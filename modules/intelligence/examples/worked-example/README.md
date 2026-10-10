# Worked example

Adopt the Intelligence module without cloning btros.

This example shows how to register the extracted seams and call them. It does not deploy anything. It does not create Azure, Entra, or Stripe resources. It does not use a product HTTP `/v1` gateway.

Sources: btros main `a04a31a`, [btros#472](https://github.com/ark360-hq/btros/pull/472) and [btros#473](https://github.com/ark360-hq/btros/pull/473). Extract, not a fork.

## Register

```csharp
using Arkos.Intelligence.Infrastructure;

builder.Services.AddArkosIntelligence(builder.Configuration, builder.Environment.EnvironmentName);
```

Development and Testing get stubs that perform no outbound HTTP. Other environments register the Azure HttpClient adapters. A partial or illegal `AzureOpenAI:*` or `AzureAiSearch:*` binding fails startup. An absent binding does not.

## Bind (customer tenant)

Set these in the adopter secret store. Do not commit secrets.

```
AzureOpenAI:Endpoint=https://{resource}.openai.azure.com/
AzureOpenAI:Deployment={regional-standard-deployment}
AzureOpenAI:Region=australiaeast
AzureOpenAI:TenantMode=Client
AzureOpenAI:DeploymentType=Standard
AzureOpenAI:ApiKey=   # omit to use DefaultAzureCredential

AzureAiSearch:Endpoint=https://{service}.search.windows.net/
AzureAiSearch:IndexName={index}
AzureAiSearch:Region=australiaeast
AzureAiSearch:TenantMode=Client
AzureAiSearch:ApiKey=   # omit to use DefaultAzureCredential
```

Provision those Azure resources in **your** subscription. This repository does not provision them.

## Call the seams

```csharp
public sealed class DraftingService(ILanguageModelClient models, IRagClient rag)
{
    public async Task<string> DraftAsync(Guid siteId, string question, CancellationToken cancellationToken)
    {
        var grounded = await rag.GroundAsync(new RetrievalQuery(siteId, question), cancellationToken);
        var completion = await models.CompleteAsync(
            new LanguageModelRequest(
            [
                new LanguageModelMessage("system", grounded.SystemInstruction),
                new LanguageModelMessage("user", grounded.UserMessage),
            ]),
            cancellationToken);
        return completion.Content;
    }
}
```

`IRagClient.GroundAsync` retrieves and composes. It does not call a model. `ILanguageModelClient.CompleteAsync` is the only production chat path.

Do not add `Azure.AI.OpenAI` or `Azure.Search.Documents` to domain code.

## What success looks like

- Development resolves `StubLanguageModelClient` and `StubRetrievalClient`.
- Production resolves `AzureOpenAILanguageModelClient` and `AzureAiSearchRetrievalClient`.
- `eastus`, `Shared`, `GlobalStandard`, and `api.openai.com` fail closed.
- Every search includes `siteId eq '{guid}'`.
- Logs never contain prompt, completion, query, chunk text, or the API key.
