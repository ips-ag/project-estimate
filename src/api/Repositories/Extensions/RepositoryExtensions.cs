using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.TextGeneration;
using ProjectEstimate.Repositories.Agents.Analyst;
using ProjectEstimate.Repositories.Agents.Architect;
using ProjectEstimate.Repositories.Agents.Consultant;
using ProjectEstimate.Repositories.Agents.Developer;
using ProjectEstimate.Repositories.Configuration;
using ProjectEstimate.Repositories.Documents;
using ProjectEstimate.Repositories.Documents.Converters;
using ProjectEstimate.Repositories.Hubs;
using ProjectEstimate.Repositories.Hubs.Converters;

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
        Func<IServiceProvider, AzureOpenAIChatCompletionService> azureOpenAiFactory = sp =>
        {
            var options = sp.GetRequiredService<IOptions<AzureOpenAiSettings>>().Value;
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new AzureOpenAIChatCompletionService(
                options.DeploymentName,
                options.Endpoint,
                options.ApiKey,
                loggerFactory: loggerFactory);
        };
        services.AddScoped<IChatCompletionService>(azureOpenAiFactory);
        services.AddScoped<ITextGenerationService>(azureOpenAiFactory);
        services.AddTransient<Kernel>(sp => new Kernel(sp));
        //// consultant
        services.AddScoped<ConsultantAgent>();
        //// analyst
        // services.AddScoped<AnalystAgent>();
        services.AddScoped<AnalystAgentFactory>();
        services.AddKeyedScoped<Agent>(
            AnalystAgentFactory.AgentName,
            (sp, _) =>
            {
                var factory = sp.GetRequiredService<AnalystAgentFactory>();
                return factory.Create();
            });
        //// architect
        services.AddScoped<ArchitectAgentFactory>();
        services.AddKeyedScoped<Agent>(
            ArchitectAgentFactory.AgentName,
            (sp, _) =>
            {
                var factory = sp.GetRequiredService<ArchitectAgentFactory>();
                return factory.Create();
            });
        // services.AddScoped<ArchitectAgent>();

        //// developer
        services.AddScoped<DeveloperAgentFactory>();
        services.AddKeyedScoped<Agent>(
            DeveloperAgentFactory.AgentName,
            (sp, _) =>
            {
                var factory = sp.GetRequiredService<DeveloperAgentFactory>();
                return factory.Create();
            });
        // services.AddScoped<DeveloperAgent>();

        // interaction
        services.AddSingleton<IUserInteraction, SignalrUserInteraction>();
        services.AddSingleton<LogLevelConverter>();

        // repositories
        services.AddScoped<ContentExtractor>();
        services.AddScoped<FileTypeConverter>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        return services;
    }
}
