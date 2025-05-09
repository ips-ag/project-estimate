namespace ProjectEstimate.Domain;

/// <summary>
/// </summary>
/// <param name="Prompt">Textual input provided by the user</param>
/// <param name="FileLocation">Optional location of a file that should be included in the context</param>
public record ChatCompletionRequest(string? Prompt, string? FileLocation);
