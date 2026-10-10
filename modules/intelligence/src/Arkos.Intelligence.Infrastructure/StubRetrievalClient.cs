using Microsoft.Extensions.Logging;
using Arkos.Intelligence;

namespace Arkos.Intelligence.Infrastructure;

/// <summary>
/// SPEC-0012 REQ-003: Development/Testing stub. No outbound HTTP, no Azure credential probe.
/// </summary>
public sealed class StubRetrievalClient : IRetrievalClient
{
    private readonly ILogger<StubRetrievalClient> _logger;

    public StubRetrievalClient(ILogger<StubRetrievalClient> logger)
    {
        _logger = logger;
    }

    public Task<RetrievalResult> SearchAsync(
        RetrievalQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.SiteId == Guid.Empty)
        {
            throw new RetrievalException(
                RetrievalFailureKind.MissingSiteScope,
                "A site id is required before a search can be sent.");
        }

        _logger.LogInformation("DEV retrieval stub; no Azure AI Search call.");
        return Task.FromResult(new RetrievalResult(query.SiteId, query.SearchText, []));
    }
}
