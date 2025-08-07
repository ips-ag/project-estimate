using ProjectEstimate.Application.Converters;
using ProjectEstimate.Application.Request.Context;

namespace ProjectEstimate.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IRequestContextAccessor, RequestContextAccessor>();
        services.AddSingleton<FileTypeConverter>();
        return services;
    }
}
