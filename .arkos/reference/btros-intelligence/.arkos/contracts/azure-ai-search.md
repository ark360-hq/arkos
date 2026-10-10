# Contract: Azure AI Search documents search (SPEC-0207)

Outbound-only adapter. No inbound webhook. The host never calls a search SDK; it POSTs this REST
shape via `HttpClient`. Chat completions remain SPEC-0206.

## Summary

| Field | Value |
|---|---|
| Interface name | Azure AI Search documents search |
| Owner | @parkjadev |
| Spec | SPEC-0207 |
| Direction | outbound |
| Protocol | REST (HTTPS) |
| Auth | `api-key` header, or `Authorization: Bearer` from the container managed identity (`https://search.azure.com/.default`) |

## Binding (configuration, not provisioned by this repo)

| Key | Required | Rule |
|---|---|---|
| `AzureAiSearch:Endpoint` | yes when used | `https://{service}.search.windows.net/` |
| `AzureAiSearch:IndexName` | yes when used | Customer-created index name |
| `AzureAiSearch:Region` | yes when used | `australiaeast` or `australiasoutheast` |
| `AzureAiSearch:TenantMode` | yes when used | `Client` only |
| `AzureAiSearch:ApiKey` | no | Key Vault `AzureAiSearch--ApiKey`; omit to use managed identity |
| `AzureAiSearch:ApiVersion` | no | Default `2024-07-01` |
| `AzureAiSearch:VectorField` | no | Default `contentVector` (used only when the caller supplies a vector) |

Key Vault names use `--` for `:`. This repository does not create the Azure AI Search service
or the index.

## Expected index fields (customer-provisioned)

| Field | Type | Use |
|---|---|---|
| `id` | Edm.String (key) | Document id |
| `siteId` | Edm.String (filterable) | Mandatory site scope |
| `title` | Edm.String | Citation title |
| `content` | Edm.String (searchable) | Chunk text |
| `source` | Edm.String | Citation source |
| `contentVector` | Collection(Edm.Single) | Optional; used only when the caller supplies a vector |

## Endpoints / operations

| Method | Path | Request | Response |
|---|---|---|---|
| POST | `/indexes/{index}/docs/search?api-version={version}` | Search body | Search envelope |

## Schema stub

```yaml
openapi: "3.1.0"
info:
  title: Azure AI Search documents search (adapter subset)
  version: "0.1.0"
paths:
  /indexes/{index}/docs/search:
    post:
      parameters:
        - in: path
          name: index
          required: true
          schema: { type: string }
        - in: query
          name: api-version
          required: true
          schema: { type: string, example: "2024-07-01" }
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              required: [search, filter]
              properties:
                search: { type: string }
                filter: { type: string, description: "Adapter-built siteId eq '{guid}'" }
                top: { type: integer, minimum: 1, maximum: 20 }
                select: { type: string, example: "id,siteId,title,content,source" }
                vectorQueries:
                  type: array
                  items:
                    type: object
                    properties:
                      kind: { type: string, enum: [vector] }
                      vector: { type: array, items: { type: number } }
                      fields: { type: string }
                      k: { type: integer }
      responses:
        "200":
          description: Search hits
          content:
            application/json:
              schema:
                type: object
                properties:
                  value:
                    type: array
                    items:
                      type: object
                      properties:
                        "@search.score": { type: number }
                        id: { type: string }
                        siteId: { type: string }
                        title: { type: string }
                        content: { type: string }
                        source: { type: string }
        "401": { description: Auth failed — adapter throws, no retry }
        "429": { description: Quota — adapter throws, no retry }
        "5XX": { description: Provider fault — adapter throws, no retry }
```

## Gate reference

Satisfies constitution principle 3 and ship-gate CRT-022 for the Azure AI Search trust boundary.
