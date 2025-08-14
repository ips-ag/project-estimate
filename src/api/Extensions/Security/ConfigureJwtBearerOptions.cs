using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using ProjectEstimate.Extensions.Security.Configuration;

namespace ProjectEstimate.Extensions.Security;

internal class ConfigureJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly IOptionsMonitor<SecurityConfiguration> _options;

    public ConfigureJwtBearerOptions(IOptionsMonitor<SecurityConfiguration> options)
    {
        _options = options;
    }

    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        var settings = _options.CurrentValue.Authentication;
        options.Authority = settings.Authority;
        options.Audience = settings.Audience;
        options.RequireHttpsMetadata = false;
        options.IncludeErrorDetails = true;
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnChallenge = c =>
            {
                c.HandleResponse();
                if (!c.HttpContext.Response.HasStarted)
                {
                    c.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = async c =>
            {
                if (c.HttpContext.Response.HasStarted) return;
                c.NoResult();
                c.Response.StatusCode = StatusCodes.Status401Unauthorized;
                c.Response.ContentType = MediaTypeNames.Application.Json;
                var factory = c.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                var problem = factory.CreateProblemDetails(
                    c.HttpContext,
                    statusCode: (int)HttpStatusCode.Unauthorized,
                    title: "Authentication Failed",
                    detail: c.Exception.Message,
                    type: $"https://httpstatuses.io/{c.Response.StatusCode}");
                await c.Response.WriteAsJsonAsync(problem, c.HttpContext.RequestAborted);
            }
        };
    }
}
