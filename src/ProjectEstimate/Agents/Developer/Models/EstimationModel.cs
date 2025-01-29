using System.Text.Json.Serialization;

namespace ProjectEstimate.Agents.Developer.Models;

internal record EstimationModel
{
    [JsonPropertyName("userStories")]
    public List<UserStory> UserStories { get; set; } = [];
}
