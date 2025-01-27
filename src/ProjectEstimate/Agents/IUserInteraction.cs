namespace ProjectEstimate.Agents;

public interface IUserInteraction
{
    ValueTask<string?> ReadUserMessageAsync(CancellationToken cancel);
    ValueTask WriteAssistantMessageAsync(string message, CancellationToken cancel);
}
