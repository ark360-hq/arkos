namespace Arkos.Intelligence.Tests;

public sealed class AzureOpenAIAdapterTests
{
    [Theory]
    [InlineData("https://api.openai.com/")]
    [InlineData("https://api.openai.com/v1")]
    public void Non_azure_openai_endpoints_are_rejected(string endpoint)
    {
        AzureOpenAIGuard.EnsureCanSend(endpoint, "gpt-4o", "australiaeast", "Client", "Standard");
    }
}
