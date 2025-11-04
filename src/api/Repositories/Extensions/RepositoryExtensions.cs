using System.ClientModel;
using System.ClientModel.Primitives;
using System.Threading.Channels;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ProjectEstimate.Repositories.Agents;
using ProjectEstimate.Repositories.Agents.Analyst;
using ProjectEstimate.Repositories.Agents.Architect;
using ProjectEstimate.Repositories.Agents.Consultant;
using ProjectEstimate.Repositories.Agents.Developer;
using ProjectEstimate.Repositories.Configuration;
using ProjectEstimate.Repositories.Documents;
using ProjectEstimate.Repositories.Documents.Converters;
using ProjectEstimate.Repositories.Hubs;
using ProjectEstimate.Repositories.Hubs.Models;

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
        services.AddSingleton(_ =>
        {
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = false
            };
            return Channel.CreateBounded<ChatCompletionRequestModel>(options);
        });
        services.AddHostedService<AgentBackgroundService>();
        services.AddScoped(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AzureOpenAiSettings>>().Value;
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var openAiClient = new AzureOpenAIClient(
                new Uri(options.Endpoint),
                new ApiKeyCredential(options.ApiKey),
                new AzureOpenAIClientOptions
                {
                    ClientLoggingOptions = new ClientLoggingOptions { LoggerFactory = loggerFactory }
                });
            var chatClient = openAiClient.GetChatClient(options.DeploymentName);
            return chatClient.AsIChatClient();
        });
        //// consultant
        services.AddScoped<ConsultantAgent>();
        //// analyst
        services.AddScoped<AnalystAgentFactory>();
        services.AddKeyedScoped<AIAgent>(
            AnalystAgentFactory.AgentName,
            (sp, _) =>
            {
                var factory = sp.GetRequiredService<AnalystAgentFactory>();
                return factory.Create();
            });
        //// architect
        services.AddScoped<ArchitectAgentFactory>();
        services.AddKeyedScoped<AIAgent>(
            ArchitectAgentFactory.AgentName,
            (sp, _) =>
            {
                var factory = sp.GetRequiredService<ArchitectAgentFactory>();
                return factory.Create();
            });
        //// developer
        services.AddScoped<DeveloperAgentFactory>();
        services.AddKeyedScoped<AIAgent>(
            DeveloperAgentFactory.AgentName,
            (sp, _) =>
            {
                var factory = sp.GetRequiredService<DeveloperAgentFactory>();
                return factory.Create();
            });

        // interaction
        services.AddSingleton<IUserInteraction, SignalrUserInteraction>();

        // repositories
        services.AddScoped<ContentExtractor>();
        services.AddScoped<FileTypeConverter>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        return services;
    }
}
