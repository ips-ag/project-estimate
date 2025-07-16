namespace ProjectEstimate.Repositories.Hubs;

internal interface IUserInteraction
{
    /// <summary>
    ///     Outputs an assistant message to the user.
    /// </summary>
    /// <param name="assistant">Assistant name</param>
    /// <param name="message">Message to output</param>
    /// <param name="conversationEnd">Indicator whether this is the final message in the conversation</param>
    /// <param name="cancel">Cancellation token</param>
    /// <returns></returns>
    ValueTask MessageOutputAsync(
        string assistant,
        string message,
        bool conversationEnd = false,
        CancellationToken cancel = default);

    /// <summary>
    ///     Outputs reasoning information from an assistant to the user.
    /// </summary>
    /// <param name="assistant">Assistant name</param>
    /// <param name="message">Reasoning message to output</param>
    /// <param name="cancel">Cancellation token</param>
    /// <returns></returns>
    ValueTask ReasoningOutputAsync(
        string assistant,
        string message,
        CancellationToken cancel = default);

    /// <summary>
    ///     Asks a question to the user and waits for an answer.
    /// </summary>
    /// <param name="assistant">Name of the assistant asking the question</param>
    /// <param name="question">Question content</param>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>Non-empty string if answer was provided. Empty or null value otherwise</returns>
    ValueTask<string?> AskQuestionAsync(string assistant, string question, CancellationToken cancel = default);
}
