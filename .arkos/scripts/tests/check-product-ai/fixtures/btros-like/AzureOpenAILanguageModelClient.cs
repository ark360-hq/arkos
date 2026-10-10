using Azure.Identity;

namespace Arkos.Intelligence.Infrastructure;

public sealed class AzureOpenAILanguageModelClient
{
    public async Task CompleteAsync(string endpoint, string deployment)
    {
        AzureOpenAIGuard.EnsureCanSend(endpoint, deployment, "australiaeast", "Client", "Standard");
        var uri = new Uri($"{endpoint}openai/deployments/{deployment}/chat/completions?api-version=2024-10-21");
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("api-key", "from-config");
        _ = new DefaultAzureCredential();
        await Task.CompletedTask;
    }
}
