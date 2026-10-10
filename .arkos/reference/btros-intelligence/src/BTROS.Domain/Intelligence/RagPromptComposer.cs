using System.Text;

namespace BTROS.Domain.Intelligence;

/// <summary>
/// SPEC-0207 REQ-007: turn retrieved chunks into a grounded prompt. Does not call a model.
/// Later callers pass <see cref="RagGroundedPrompt.SystemInstruction"/> and
/// <see cref="RagGroundedPrompt.UserMessage"/> to SPEC-0206's language-model seam.
/// </summary>
public static class RagPromptComposer
{
    public const string SystemInstruction =
        "Answer using only the retrieved site documents. Cite sources by number. "
        + "If the documents do not contain the answer, say you do not know. "
        + "Do not invent documents, customers, or metrics.";

    public static RagGroundedPrompt Compose(RetrievalQuery query, RetrievalResult result)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(result);

        var citations = result.Chunks
            .Where(chunk => chunk.SiteId == query.SiteId)
            .ToArray();

        var body = new StringBuilder();
        body.Append("Question: ");
        body.AppendLine(query.SearchText.Trim());
        body.AppendLine();

        if (citations.Length == 0)
        {
            body.Append("Retrieved documents: none. Say you do not know.");
        }
        else
        {
            body.AppendLine("Retrieved documents:");
            for (var i = 0; i < citations.Length; i++)
            {
                var chunk = citations[i];
                body.Append('[');
                body.Append(i + 1);
                body.Append("] ");
                body.Append(chunk.Title);
                if (!string.IsNullOrWhiteSpace(chunk.Source))
                {
                    body.Append(" (");
                    body.Append(chunk.Source);
                    body.Append(')');
                }

                body.AppendLine();
                body.AppendLine(chunk.Content);
            }
        }

        return new RagGroundedPrompt(SystemInstruction, body.ToString().TrimEnd(), citations);
    }
}
