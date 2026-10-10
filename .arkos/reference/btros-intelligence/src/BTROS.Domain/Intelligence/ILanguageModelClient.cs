namespace BTROS.Domain.Intelligence;

/// <summary>
/// SPEC-0206 / ADR-0049: provider-neutral chat-completions boundary. Production uses the Azure
/// OpenAI adapter; Development/Testing use a stub. Domain callers must not reference Azure types.
/// </summary>
public interface ILanguageModelClient
{
    Task<LanguageModelCompletion> CompleteAsync(
        LanguageModelRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record LanguageModelMessage(string Role, string Content);

public sealed record LanguageModelRequest(
    IReadOnlyList<LanguageModelMessage> Messages,
    int? MaxTokens = null,
    double? Temperature = null);

public sealed record LanguageModelCompletion(string Content, string Deployment, string Region);

public enum LanguageModelFailureKind
{
    NotConfigured,
    NonAustralianRegion,
    NonClientTenant,
    InvalidEndpoint,
    InvalidDeploymentType,
    RequestFailed,
}

/// <summary>Fail-closed model-adapter error. Never a silent skip.</summary>
public sealed class LanguageModelException : Exception
{
    public LanguageModelFailureKind Kind { get; }

    public LanguageModelException(LanguageModelFailureKind kind, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Kind = kind;
    }
}
