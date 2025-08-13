using ProjectEstimate.Extensions.Cors.Configuration;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace ProjectEstimate.Extensions.Cors;

public class ConfigureCorsOptions : IPostConfigureOptions<CorsOptions>
{
    private readonly IOptionsMonitor<CorsSettings> _options;

    public ConfigureCorsOptions(IOptionsMonitor<CorsSettings> options)
    {
        _options = options;
    }

    public void PostConfigure(string? name, CorsOptions options)
    {
        var configuration = _options.CurrentValue;
        if (!configuration.UseCors) return;
        if (configuration.AllowedOrigins is null) return;
        options.AddPolicy(
            configuration.PolicyName,
            policy => policy
                .WithOrigins(configuration.AllowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
    }
}
