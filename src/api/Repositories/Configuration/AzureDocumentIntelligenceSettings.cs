namespace ProjectEstimate.Repositories.Configuration;

public class AzureDocumentIntelligenceSettings
{
    public const string SectionName = "Azure:DocumentIntelligence";
    public required string Endpoint { get; set; }
    public required string ApiKey { get; set; }
}
