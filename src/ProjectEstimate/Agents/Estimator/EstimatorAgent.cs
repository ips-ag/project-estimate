using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ProjectEstimate.Configuration;
using Serilog;

namespace ProjectEstimate.Agents.Estimator;

internal class EstimatorAgent
{
    private readonly IOptionsMonitor<AzureOpenAiSettings> _options;
    private Kernel _kernel = null!;
    private IChatCompletionService _chatCompletionService = null!;
    private ChatHistory _history = null!;
    private OpenAIPromptExecutionSettings _openAiPromptExecutionSettings = null!;

    public EstimatorAgent(IOptionsMonitor<AzureOpenAiSettings> options)
    {
        _options = options;
        Initialize();
    }

    /// <summary>
    ///     Reads user input from the console. Writes agent output to the console.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>true if stopping condition was met, false otherwise</returns>
    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        await Console.Out.WriteAsync("User > ");
        string? userInput = await Console.In.ReadLineAsync(cancellationToken);
        if (string.IsNullOrEmpty(userInput)) return true;

        // Add user input
        _history.AddUserMessage(userInput);

        // Get the response from the AI
        var result = await _chatCompletionService.GetChatMessageContentAsync(
            _history,
            executionSettings: _openAiPromptExecutionSettings,
            kernel: _kernel,
            cancellationToken: cancellationToken);

        // Print the results
        await Console.Out.WriteLineAsync("Assistant > " + result);

        // Add the message from the agent to the chat history
        _history.AddMessage(result.Role, result.Content ?? string.Empty);
        return false;
    }

    private void Initialize()
    {
        // Populate values from your OpenAI deployment
        var settings = _options.CurrentValue;
        string modelId = settings.DeploymentName;
        string endpoint = settings.Endpoint;
        string apiKey = settings.ApiKey;

        // Create a kernel with Azure OpenAI chat completion
        var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);

        // Add enterprise components
        builder.Services.AddSerilog();

        // Build the kernel
        _kernel = builder.Build();
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        // Add a plugin (the LightsPlugin class is defined below)
        // _kernel.Plugins.AddFromType<LightsPlugin>("Lights");
        // _kernel.Plugins.AddFromType<CalculatorPlugin>("Calculator");
        // Enable planning
        _openAiPromptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ChatSystemPrompt =
                """
                Assistant is a software development project estimator. It helps estimating the time and cost of software development projects.
                Input consists of all gathered requirements for a software project. They can be functional or non-functional requirements.
                Estimates are provided based on the project requirements and input from the architecture team.
                Estimates are provided for each functional requirement in man-days.
                Provide breakdown and explanation of the estimates for each functional requirement.
                Do not answer questions that are not related to software development project estimation.
                """
        };

        // Create a history store the conversation
        _history = [];
    }
}
