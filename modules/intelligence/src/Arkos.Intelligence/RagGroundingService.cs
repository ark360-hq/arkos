namespace Arkos.Intelligence;

/// <summary>SPEC-0012: retrieve, then compose. No language-model call.</summary>
public sealed class RagGroundingService : IRagClient
{
    private readonly IRetrievalClient _retrieval;

    public RagGroundingService(IRetrievalClient retrieval)
    {
        _retrieval = retrieval;
    }

    public async Task<RagGroundedPrompt> GroundAsync(
        RetrievalQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var result = await _retrieval.SearchAsync(query, cancellationToken).ConfigureAwait(false);
        return RagPromptComposer.Compose(query, result);
    }
}
