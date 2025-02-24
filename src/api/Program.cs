using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ProjectEstimate.Repositories.Agents;
using ProjectEstimate.Repositories.Configuration;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(
        builder => builder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("secrets.json", optional: true, reloadOnChange: true))
    .ConfigureServices(
        services =>
        {
            services.AddApplicationInsightsTelemetryWorkerService();
            services.ConfigureFunctionsApplicationInsights();

            services.AddOptions<AzureOpenAiSettings>()
                .BindConfiguration(AzureOpenAiSettings.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddSingleton<IChatCompletionService>(
                sp =>
                {
                    var options = sp.GetRequiredService<IOptions<AzureOpenAiSettings>>().Value;
                    return new AzureOpenAIChatCompletionService(
                        options.DeploymentName,
                        options.Endpoint,
                        options.ApiKey);
                });
            services.AddKeyedTransient<Kernel>(
                "TestAgentKernel",
                (sp, key) => new Kernel(sp));
            services.AddScoped<TestAgent>();
        })
    .Build();

host.Run();
