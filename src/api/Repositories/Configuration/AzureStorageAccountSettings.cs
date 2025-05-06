namespace ProjectEstimate.Repositories.Configuration;

public class AzureStorageAccountSettings
{
    public const string SectionName = "Azure:StorageAccount";
    
    public required string ConnectionString { get; set; }
}
