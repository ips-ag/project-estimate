using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ProjectEstimate.Repositories.Agents.Analyst;
using ProjectEstimate.Repositories.Agents.Architect;
using ProjectEstimate.Repositories.Agents.Consultant;
using ProjectEstimate.Repositories.Agents.Developer;
using ProjectEstimate.Repositories.Configuration;
using ProjectEstimate.Repositories.Documents;
using ProjectEstimate.Repositories.Documents.Converters;
using ProjectEstimate.Repositories.Hubs;

namespace ProjectEstimate.Repositories.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // configuration
        services.AddOptions<AzureStorageAccountSettings>()
            .BindConfiguration(AzureStorageAccountSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<AzureOpenAiSettings>()
            .BindConfiguration(AzureOpenAiSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<AzureDocumentIntelligenceSettings>()
            .BindConfiguration(AzureDocumentIntelligenceSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // agents
        services.AddSingleton<IChatCompletionService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AzureOpenAiSettings>>().Value;
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new AzureOpenAIChatCompletionService(
                options.DeploymentName,
                options.Endpoint,
                options.ApiKey,
                loggerFactory: loggerFactory);
        });
        services.AddKeyedTransient<Kernel>("ConsultantAgent", (sp, _) => new Kernel(sp));
        services.AddScoped<ConsultantAgent>();
        services.AddKeyedTransient<Kernel>("AnalystAgent", (sp, _) => new Kernel(sp));
        services.AddScoped<AnalystAgent>();
        services.AddKeyedTransient<Kernel>("ArchitectAgent", (sp, _) => new Kernel(sp));
        services.AddScoped<ArchitectAgent>();
        services.AddKeyedTransient<Kernel>("DeveloperAgent", (sp, _) => new Kernel(sp));
        services.AddScoped<DeveloperAgent>();

        // interaction
        services.AddSingleton<IUserInteraction, SignalrUserInteraction>();

        // repositories
        services.AddScoped<ContentExtractor>();
        services.AddScoped<FileTypeConverter>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        return services;
    }
}
