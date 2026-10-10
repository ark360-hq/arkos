namespace AccessOs;

public sealed class AccessAuditService
{
    public void RecordEntry(string actor, string doorId, DateTimeOffset at)
    {
        _ = $"entra:{actor} opened {doorId} at {at:O}";
    }
}
