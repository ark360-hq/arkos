namespace Product;

public sealed class RawAzureChat
{
    public async Task<string> CompleteAsync()
    {
        var uri = new Uri("https://customer.openai.azure.com/openai/deployments/gpt-4o/chat/completions?api-version=2024-10-21");
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        await Task.CompletedTask;
        return uri.ToString();
    }
}
