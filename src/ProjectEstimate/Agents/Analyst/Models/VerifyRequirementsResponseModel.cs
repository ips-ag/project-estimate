using System.Text.Json.Serialization;

namespace ProjectEstimate.Agents.Analyst.Models;

internal class VerifyRequirementsResponseModel
{
    [JsonPropertyName("requirementsComplete")]
    [JsonRequired]
    public bool RequirementsComplete { get; set; }

    [JsonPropertyName("questions")]
    public List<QuestionModel> Questions { get; set; } = [];
}
