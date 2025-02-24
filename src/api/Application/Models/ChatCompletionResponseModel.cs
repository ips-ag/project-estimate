using System.Text.Json.Serialization;

namespace ProjectEstimate.Application.Models;

public class ChatCompletionResponseModel
{
    [JsonPropertyName("output")]
    public string? Output { get; set; }

    [JsonPropertyName("responseRequired")]
    public bool ResponseRequired { get; set; } = false;
}
