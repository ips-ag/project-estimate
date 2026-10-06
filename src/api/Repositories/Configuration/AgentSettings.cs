namespace ProjectEstimate.Repositories.Configuration;

public class AgentSettings
{
    public static string SectionName(string agentName) => $"Agents:{agentName}";
    public ReasoningEffort ReasoningEffort { get; set; } = ReasoningEffort.Minimal;
}
