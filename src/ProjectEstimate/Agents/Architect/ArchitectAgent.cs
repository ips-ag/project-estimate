using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ProjectEstimate.Agents.Architect.Models;
using ProjectEstimate.Configuration;
using Serilog;

namespace ProjectEstimate.Agents.Architect;

internal class ArchitectAgent
{
    private readonly IOptionsMonitor<AzureOpenAiSettings> _options;
    private readonly IUserInteraction _userInteraction;
    private Kernel _kernel = null!;
    private IChatCompletionService _chatCompletionService = null!;
    private OpenAIPromptExecutionSettings _openAiPromptExecutionSettings = null!;

    public ArchitectAgent(IOptionsMonitor<AzureOpenAiSettings> options, IUserInteraction userInteraction)
    {
        _options = options;
        _userInteraction = userInteraction;
        Initialize();
    }

    public async ValueTask<EstimationModel?> EstimateAsync(ChatHistory history, CancellationToken cancel)
    {
        var result = await _chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: _openAiPromptExecutionSettings,
            kernel: _kernel,
            cancellationToken: cancel);
        if (result.Content is null) return null;
        history.AddAssistantMessage(result.Content);
        try
        {
            return JsonSerializer.Deserialize<EstimationModel>(result.Content);
        }
        catch (JsonException)
        {
            await _userInteraction.WriteAssistantMessageAsync(result.Content, cancel);
            return null;
        }
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
                Assistant is an experienced software architects. It estimates effort needed for project delivery, based on requirements.
                Input consists of all gathered requirements for a software project. They can be functional or non-functional requirements.
                Output consists of identified user-stories, tasks, and estimated time for each task.
                Estimates are provided for each task in man-days. Estimate can be fractional, e.g. 0.25, 0.5, 1.25, etc.
                Provide optimistic, pessimistic, and realistic estimate for each task. Optimistic estimate should be less or equal than realistic, and realistic must be less or equal than pessimistic.
                Output should be in JSON format. Include only JSON object, without any additional text.
                Example output:
                {
                    "userStories": [
                        {
                            "name": "User story title",
                            "tasks": [
                                {
                                    "name": "Task title",
                                    "optimistic": 1.0,
                                    "pessimistic": 2.0,
                                    "realistic": 1.5
                                }
                            ]
                        }
                    ]
                }
                Do not answer requests that are not related to software project delivery estimation.
                """
        };
    }
}
