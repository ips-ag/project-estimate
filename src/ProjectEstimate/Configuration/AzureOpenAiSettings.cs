namespace ProjectEstimate.Configuration;

public class AzureOpenAiSettings
{
    public required string Endpoint { get; set; }
    public required string ApiKey { get; set; }
    public required string DeploymentName { get; set; }
}
