using System.Text.Json.Serialization;

namespace ProjectEstimate.Agents.Architect.Models;

internal record EstimationModel
{
    [JsonPropertyName("userStories")]
    public List<UserStory> UserStories { get; set; } = [];
}
