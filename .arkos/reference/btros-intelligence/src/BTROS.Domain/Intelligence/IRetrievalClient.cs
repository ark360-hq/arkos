namespace BTROS.Domain.Intelligence;

/// <summary>
/// SPEC-0207 / ADR-0050: provider-neutral retrieval boundary. Production uses the Azure AI
/// Search adapter; Development/Testing use a stub. Domain callers must not reference Azure types.
/// </summary>
public interface IRetrievalClient
{
    Task<RetrievalResult> SearchAsync(
        RetrievalQuery query,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// SPEC-0207: retrieve-then-ground. Composes a prompt for a later SPEC-0206
/// <c>ILanguageModelClient</c> call. Does not invoke a model.
/// </summary>
public interface IRagClient
{
    Task<RagGroundedPrompt> GroundAsync(
        RetrievalQuery query,
        CancellationToken cancellationToken = default);
}

public sealed record RetrievalQuery(
    Guid SiteId,
    string SearchText,
    int Top = 5,
    IReadOnlyList<float>? Vector = null);

public sealed record RetrievedChunk(
    string Id,
    Guid SiteId,
    string Title,
    string Content,
    string Source,
    double Score);

public sealed record RetrievalResult(
    Guid SiteId,
    string SearchText,
    IReadOnlyList<RetrievedChunk> Chunks);

public sealed record RagGroundedPrompt(
    string SystemInstruction,
    string UserMessage,
    IReadOnlyList<RetrievedChunk> Citations);

public enum RetrievalFailureKind
{
    NotConfigured,
    NonAustralianRegion,
    NonClientTenant,
    InvalidEndpoint,
    MissingSiteScope,
    RequestFailed,
}

/// <summary>Fail-closed retrieval-adapter error. Never a silent skip.</summary>
public sealed class RetrievalException : Exception
{
    public RetrievalFailureKind Kind { get; }

    public RetrievalException(RetrievalFailureKind kind, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Kind = kind;
    }
}
