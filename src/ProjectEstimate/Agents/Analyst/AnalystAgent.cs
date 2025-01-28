using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ProjectEstimate.Agents.Analyst.Models;
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

    public async ValueTask<VerifyRequirementsResponseModel?> VerifyRequirementsAsync(
        ChatHistory history,
        CancellationToken cancel)
    {
        var result = await _chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: _openAiPromptExecutionSettings,
            kernel: _kernel,
            cancellationToken: cancel);
        if (result.Content is null) return null;
        history.AddAssistantMessage(result.Content);
        return JsonSerializer.Deserialize<VerifyRequirementsResponseModel>(result.Content);
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
                Assistant is a business analysts. It verifies project requirements.
                Input consists of all gathered requirements for a software project. They can be functional or non-functional requirements.
                Output is in the JSON format. It consists of a boolean field 'requirementsComplete' and optional array of questions. Each question contains a value and explanation.
                Maximum 2 questions can be asked.
                Output example when requirements are not complete:
                {
                  "requirementsComplete": false,
                  "questions": [
                    {
                      "value": "What is the target audience for the software?",
                      "explanation": "This question is important to understand the user base and design the software accordingly."
                    },
                    {
                      "value": "What is the expected load on the system?",
                      "explanation": "This question is important to design the system architecture and estimate the hardware requirements."
                    }
                  ]
                }
                Output example when requirements are complete:
                {
                  "requirementsComplete": true
                }
                Provide explanation of each question in the output.
                Do not answer requests that are not related to project requirements analysis.
                """
        };

        // Create a history store the conversation
    }
}
