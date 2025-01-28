using System.Text.Json.Serialization;

namespace ProjectEstimate.Agents.Analyst.Models;

internal class QuestionModel
{
    [JsonPropertyName("value")]
    [JsonRequired]
    public required string Value { get; set; }

    [JsonPropertyName("explanation")]
    public required string Explanation { get; set; }
}
