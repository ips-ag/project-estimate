﻿using System.Text.Json.Serialization;

namespace ProjectEstimate.Application.Models;

public class ChatCompletionRequestModel
{
    [JsonPropertyName("input")]
    public string? Input { get; set; }

    [JsonPropertyName("fileInput")]
    public string? FileInput { get; set; }

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }
}
