using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ProjectEstimate.Configuration;
using Serilog;

namespace ProjectEstimate.Agents.Analyst;

internal class AnalystAgent
{
    private readonly IOptionsMonitor<AzureOpenAiSettings> _options;
    private Kernel _kernel = null!;
    private IChatCompletionService _chatCompletionService = null!;
    private OpenAIPromptExecutionSettings _openAiPromptExecutionSettings = null!;

    public AnalystAgent(IOptionsMonitor<AzureOpenAiSettings> options)
    {
        _options = options;
        Initialize();
    }

    public async ValueTask<string?> VerifyRequirementsAsync(ChatHistory history, CancellationToken cancel)
    {
        var result = await _chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: _openAiPromptExecutionSettings,
            kernel: _kernel,
            cancellationToken: cancel);
        return result.Content;
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

        // Enable planning
        _openAiPromptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ChatSystemPrompt =
                """
                Assistant is a business analysts. It to verify project requirements.
                Input consists of all gathered requirements for a software project. They can be functional or non-functional requirements.
                If requirements are not complete, output consists of a message 'Requirements not clear' and list of additional questions that need answering before project can be estimated.
                If no questions need answering, the output should only say 'Requirement verification complete'.
                Provide breakdown and explanation of each question in the output.
                Do not answer requests that are not related to project requirements analysis.
                """
        };

        // Create a history store the conversation
    }
}
