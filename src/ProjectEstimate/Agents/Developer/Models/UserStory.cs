using System.Text.Json.Serialization;

namespace ProjectEstimate.Agents.Developer.Models;

internal class UserStory
{
    [JsonPropertyName("name")]
    [JsonRequired]
    public required string Name { get; set; }

    [JsonPropertyName("tasks")]
    public List<TaskModel> Tasks { get; set; } = [];
}