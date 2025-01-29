using System.Text.Json.Serialization;

namespace ProjectEstimate.Agents.Architect.Models;

internal class TaskModel
{
    [JsonPropertyName("name")]
    [JsonRequired]
    public required string Name { get; set; }

    [JsonPropertyName("optimistic")]
    [JsonRequired]
    public required double Optimistic { get; set; }

    [JsonPropertyName("pessimistic")]
    [JsonRequired]
    public required double Pessimistic { get; set; }

    [JsonPropertyName("realistic")]
    [JsonRequired]
    public required double Realistic { get; set; }
}