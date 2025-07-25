using System.Threading.Channels;
using ProjectEstimate.Application.Agents;
using ProjectEstimate.Application.Converters;
using ProjectEstimate.Application.Models;
using ProjectEstimate.Application.Request.Context;

namespace ProjectEstimate.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IRequestContextAccessor, RequestContextAccessor>();
        services.AddSingleton<FileTypeConverter>();
        services.AddSingleton(_ =>
        {
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = false
            };
            return Channel.CreateBounded<ChatCompletionRequestModel>(options);
        });
        services.AddHostedService<AgentBackgroundService>();
        return services;
    }
}
