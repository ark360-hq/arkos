# Intelligence module source map

Extract, not a fork. SPEC-0012 REQ-002 and REQ-007.

| Landed source | What was extracted |
|---|---|
| [ark360-hq/btros#472](https://github.com/ark360-hq/btros/pull/472) Azure OpenAI adapter | `bicep/openai.bicep` (OpenAI account, chat deployment, embeddings deployment, endpoint-only outputs, local auth disabled) and the gateway `complete` operation |
| [ark360-hq/btros#473](https://github.com/ark360-hq/btros/pull/473) AI Search RAG slice | `bicep/search.bicep` (Search service, retrieve index name, endpoint-only outputs, local auth disabled) and the gateway `retrieve` operation |

Shared extract:

- `.arkos/contracts/intelligence-gateway.md`
- `.arkos/contracts/intelligence-gateway.openapi.yaml`
- Audit record required on every gateway operation

Not extracted (do not fork these into ARK OS):

- The btros operator console
- Resident, payments, or access-control product features
- Entra or Stripe resources
- Any deploy step that would create a billable resource from this repository

This GitHub App installation cannot read the private `ark360-hq/btros` tree (repository 404). James Park named #472 and #473 as the landed sources on btros main. The files in this module are the SPEC-0012 extract of that named surface. They are not a copy of the btros application. If a later run can read those pulls and the Bicep differs, replace these files with a verbatim extract and keep the audit-call obligation.
