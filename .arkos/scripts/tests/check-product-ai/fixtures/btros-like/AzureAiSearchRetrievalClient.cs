namespace Arkos.Intelligence.Infrastructure;

public sealed class AzureAiSearchRetrievalClient
{
    public async Task SearchAsync(string endpoint, string indexName, Guid siteId)
    {
        AzureAiSearchGuard.EnsureCanSend(endpoint, indexName, "australiasoutheast", "Client", siteId);
        var top = AzureAiSearchGuard.ClampTop(5);
        var uri = new Uri($"{endpoint}indexes/{indexName}/docs/search?api-version=2024-07-01");
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        _ = top;
        await Task.CompletedTask;
    }
}
