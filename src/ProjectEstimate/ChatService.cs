using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ProjectEstimate.Configuration;
using ProjectEstimate.Plugins;
using Serilog;

namespace ProjectEstimate;

public class ChatService : BackgroundService
{
    private readonly IOptions<AzureOpenAiSettings> _options;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public ChatService(IOptions<AzureOpenAiSettings> options, IHostApplicationLifetime applicationLifetime)
    {
        _options = options;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Populate values from your OpenAI deployment
            var settings = _options.Value;
            string modelId = settings.DeploymentName;
            string endpoint = settings.Endpoint;
            string apiKey = settings.ApiKey;

            // Create a kernel with Azure OpenAI chat completion
            var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);

            // Add enterprise components
            builder.Services.AddSerilog();

            // Build the kernel
            var kernel = builder.Build();
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Add a plugin (the LightsPlugin class is defined below)
            kernel.Plugins.AddFromType<LightsPlugin>("Lights");

            // Enable planning
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            // Create a history store the conversation
            var history = new ChatHistory();

            // Initiate a back-and-forth chat
            while (!stoppingToken.IsCancellationRequested)
            {
                // Collect user input
                await Console.Out.WriteAsync("User > ");
                string? userInput = await Console.In.ReadLineAsync(stoppingToken);
                if (string.IsNullOrEmpty(userInput)) break;

                // Add user input
                history.AddUserMessage(userInput);

                // Get the response from the AI
                var result = await chatCompletionService.GetChatMessageContentAsync(
                    history,
                    executionSettings: openAIPromptExecutionSettings,
                    kernel: kernel,
                    cancellationToken: stoppingToken);

                // Print the results
                await Console.Out.WriteLineAsync("Assistant > " + result);

                // Add the message from the agent to the chat history
                history.AddMessage(result.Role, result.Content ?? string.Empty);
            }
        }
        finally
        {
            _applicationLifetime.StopApplication();
        }
    }
}
