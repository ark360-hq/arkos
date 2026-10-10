# Worked example

Adopt the Intelligence module without cloning btros.

This example composes the extracted Azure OpenAI and Azure AI Search Bicep and shows how a product calls the governed gateway. It does not deploy anything. It does not create Azure, Entra, or Stripe resources.

Sources: [btros#472](https://github.com/ark360-hq/btros/pull/472) and [btros#473](https://github.com/ark360-hq/btros/pull/473). Extract, not a fork.

## Files

| File | Role |
|---|---|
| [main.bicep](main.bicep) | Composes `openai.bicep` and `search.bicep` |
| [parameters.example.json](parameters.example.json) | Placeholder parameters. Replace every `REPLACE_*` value in your tenant |
| [gateway-calls.md](gateway-calls.md) | How to call complete, retrieve, and tools through the gateway |
| [audit-record.example.json](audit-record.example.json) | Shape of the required audit record |

## Steps

1. Copy this folder and `../../bicep/` into your product repository.
2. Edit a private parameters file. Do not commit secrets. Use your secret store for credentials.
3. Review [gateway-calls.md](gateway-calls.md). Wire your product AI client to `POST /v1/complete` and `POST /v1/retrieve`.
4. Do not add an Azure OpenAI or Azure AI Search SDK call from the product. The gateway is the only path.
5. If you deploy, do it in your tenant. Do not deploy from this ARK OS repository.

## What success looks like

- Bicep outputs two endpoints and an index name.
- A completion request returns `output` and a required `audit` object.
- A retrieval request returns `hits` and a required `audit` object.
- A tool invocation returns `result` and a required `audit` object.
- No raw provider SDK is on the product AI path.
