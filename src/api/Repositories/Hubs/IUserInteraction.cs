namespace ProjectEstimate.Repositories.Hubs;

internal interface IUserInteraction
{
    ValueTask WriteAssistantMessageAsync(
        string assistant,
        string message,
        CancellationToken cancel,
        LogLevel logLevel = LogLevel.Information);

    ValueTask<string?> AskQuestionAsync(string assistant, string question, CancellationToken cancel);
}
