# Contract: Azure OpenAI Chat Completions (SPEC-0012)

Outbound-only adapter. No inbound webhook. The host never calls a model SDK; it POSTs this REST
shape via `HttpClient`. Extracted from btros main `a04a31a` (#472).

## Summary

| Field | Value |
|---|---|
| Interface name | Azure OpenAI Chat Completions |
| Owner | @ark360-hq |
| Spec | SPEC-0012 |
| Direction | outbound |
| Protocol | REST (HTTPS) |
| Auth | `api-key` header, or `Authorization: Bearer` from `DefaultAzureCredential` (`https://cognitiveservices.azure.com/.default`) |

## Binding (configuration, not provisioned by this repo)

| Key | Required | Rule |
|---|---|---|
| `AzureOpenAI:Endpoint` | yes when used | `https://{resource}.openai.azure.com/` |
| `AzureOpenAI:Deployment` | yes when used | Customer-created **regional Standard** deployment name |
| `AzureOpenAI:Region` | yes when used | `australiaeast` or `australiasoutheast` |
| `AzureOpenAI:DeploymentType` | yes when used | `Standard` only (not Global / Data Zone / Provisioned) |
| `AzureOpenAI:TenantMode` | yes when used | `Client` only |
| `AzureOpenAI:ApiKey` | no | Omit to use `DefaultAzureCredential` |
| `AzureOpenAI:ApiVersion` | no | Default `2024-10-21` |

This repository does not create the Azure OpenAI account.

## Endpoints / operations

| Method | Path | Request | Response |
|---|---|---|---|
| POST | `/openai/deployments/{deployment}/chat/completions?api-version={version}` | Chat completion body | Chat completion envelope |

## Schema stub

```yaml
openapi: "3.1.0"
info:
  title: Azure OpenAI Chat Completions (adapter subset)
  version: "0.1.0"
paths:
  /openai/deployments/{deployment}/chat/completions:
    post:
      parameters:
        - in: path
          name: deployment
          required: true
          schema: { type: string }
        - in: query
          name: api-version
          required: true
          schema: { type: string, example: "2024-10-21" }
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              required: [messages]
              properties:
                messages:
                  type: array
                  items:
                    type: object
                    required: [role, content]
                    properties:
                      role: { type: string, enum: [system, user, assistant] }
                      content: { type: string }
                max_tokens: { type: integer }
                temperature: { type: number }
      responses:
        "200":
          description: Completion
          content:
            application/json:
              schema:
                type: object
                properties:
                  choices:
                    type: array
                    items:
                      type: object
                      properties:
                        message:
                          type: object
                          properties:
                            content: { type: string }
        "401": { description: Auth failed. Adapter throws, no retry }
        "429": { description: Quota. Adapter throws, no retry }
        "5XX": { description: Provider fault. Adapter throws, no retry }
```

## Gate reference

Satisfies constitution principle 3 and ship-gate CRT-022 for the Azure OpenAI trust boundary.
