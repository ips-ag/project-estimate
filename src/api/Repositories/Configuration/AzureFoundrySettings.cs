namespace ProjectEstimate.Repositories.Configuration;

public class AzureFoundrySettings
{
    public const string SectionName = "Azure:Foundry";
    public required string Endpoint { get; set; }
    public required string ApiKey { get; set; }
    public required string DeploymentName { get; set; }
}
