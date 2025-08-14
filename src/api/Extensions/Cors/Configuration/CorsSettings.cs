namespace ProjectEstimate.Extensions.Cors.Configuration;

public class CorsSettings
{
    public const string SectionName = "Cors";
    public required string PolicyName { get; init; }
    public string[]? AllowedOrigins { get; init; }

    public bool UseCors
    {
        get => AllowedOrigins is not null && AllowedOrigins.Length > 0;
    }
}
