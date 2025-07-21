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
    ///     Waits for an answer from a user.
    /// </summary>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>Non-empty string if answer was provided. Empty or null value otherwise</returns>
    ValueTask<string?> GetAnswerAsync(CancellationToken cancel = default);
}
