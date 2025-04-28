namespace ProjectEstimate.Repositories.Hubs;

internal interface IUserInteraction
{
    ValueTask WriteAssistantMessageAsync(string assistant, string message, CancellationToken cancel);
}
