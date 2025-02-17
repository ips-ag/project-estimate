namespace ProjectEstimate.Repositories.Configuration;

public class AzureOpenAiSettings
{
    public const string SectionName = "Azure:OpenAi";
    public required string Endpoint { get; set; }
    public required string ApiKey { get; set; }
    public required string DeploymentName { get; set; }
}
