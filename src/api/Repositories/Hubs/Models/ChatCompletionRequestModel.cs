namespace ProjectEstimate.Repositories.Hubs.Models;

public class ChatCompletionRequestModel
{
    public string? Prompt { get; set; }
    public string? FileLocation { get; set; }
    public required string ConnectionId { get; set; }
}
