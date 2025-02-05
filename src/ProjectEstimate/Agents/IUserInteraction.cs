namespace ProjectEstimate.Agents;

public interface IUserInteraction
{
    ValueTask<string?> ReadUserMessageAsync(CancellationToken cancel);
    ValueTask<string?> ReadMultilineUserMessageAsync(CancellationToken cancel);
    ValueTask WriteAssistantMessageAsync(string role, string message, CancellationToken cancel);
}
