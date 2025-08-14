using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using ProjectEstimate.Extensions.Security.Configuration;

namespace ProjectEstimate.Extensions.Security;

public static class SecurityExtensions
{
    public static void ConfigureAuthentication(this IServiceCollection services)
    {
        services.AddOptions<SecurityConfiguration>()
            .BindConfiguration(SecurityConfiguration.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddTransient<IConfigureOptions<AuthenticationOptions>, ConfigureAuthenticationOptions>();
        services.AddTransient<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
        services.AddAuthentication().AddJwtBearer();
    }
}
