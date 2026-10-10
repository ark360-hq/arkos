namespace Product;

public sealed class RawSearch
{
    public async Task SearchAsync()
    {
        var uri = new Uri("https://customer.search.windows.net/indexes/docs/docs/search?api-version=2024-07-01");
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        await Task.CompletedTask;
    }
}
