using System.Text.Json.Serialization;

namespace ProjectEstimate.Application.Models;

public class ChatCompletionRequestModel
{
    [JsonPropertyName("input")]
    public string? Input { get; set; }
}
