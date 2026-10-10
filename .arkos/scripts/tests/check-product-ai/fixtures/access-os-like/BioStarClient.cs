namespace AccessOs;

public sealed class BioStarClient
{
    public Task UnlockAsync(string readerId)
    {
        _ = readerId;
        return Task.CompletedTask;
    }
}
