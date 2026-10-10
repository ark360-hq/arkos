using Microsoft.Extensions.Logging;
using BTROS.Domain.Intelligence;

namespace BTROS.Infrastructure.Intelligence;

/// <summary>
/// SPEC-0206 REQ-003: Development/Testing stub. No outbound HTTP, no Azure credential probe.
/// </summary>
public sealed class StubLanguageModelClient : ILanguageModelClient
{
    private readonly ILogger<StubLanguageModelClient> _logger;

    public StubLanguageModelClient(ILogger<StubLanguageModelClient> logger)
    {
        _logger = logger;
    }

    public Task<LanguageModelCompletion> CompleteAsync(
        LanguageModelRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.LogInformation("DEV language-model stub; no Azure OpenAI call.");
        return Task.FromResult(new LanguageModelCompletion(
            "stub",
            "stub",
            AzureOpenAIGuard.AllowedRegions[0]));
    }
}
