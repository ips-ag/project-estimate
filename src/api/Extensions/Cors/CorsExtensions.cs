using ProjectEstimate.Extensions.Cors.Configuration;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace ProjectEstimate.Extensions.Cors;

public static class CorsExtensions
{
    public static IServiceCollection ConfigureCors(this IServiceCollection services)
    {
        services.AddOptions<CorsSettings>().BindConfiguration(CorsSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddTransient<IPostConfigureOptions<CorsOptions>, ConfigureCorsOptions>();
        services.AddCors();
        return services;
    }

    public static IApplicationBuilder UseCorsMiddleware(this IApplicationBuilder app)
    {
        var settings = app.ApplicationServices.GetRequiredService<IOptions<CorsSettings>>().Value;
        if (settings.UseCors)
        {
            app.UseCors(settings.PolicyName);
        }
        return app;
    }
}
