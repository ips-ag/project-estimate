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
    private const string RoleName = "Analyst";
    private readonly IOptionsMonitor<AzureOpenAiSettings> _options;
    private readonly IUserInteraction _userInteraction;
    private Kernel _kernel = null!;
    private IChatCompletionService _chatCompletionService = null!;
    private OpenAIPromptExecutionSettings _openAiPromptExecutionSettings = null!;

    public AnalystAgent(IOptionsMonitor<AzureOpenAiSettings> options, IUserInteraction userInteraction)
    {
        _options = options;
        _userInteraction = userInteraction;
        Initialize();
    }

    public async ValueTask<List<RequirementVerificationModel>> VerifyRequirementsAsync(
        ChatHistory history,
        CancellationToken cancel)
    {
        List<RequirementVerificationModel> verifications = [];
        do
        {
            var result = await _chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: _openAiPromptExecutionSettings,
                kernel: _kernel,
                cancellationToken: cancel);
            if (result.Content is null) break;
            history.AddAssistantMessage(result.Content);
            await _userInteraction.WriteAssistantMessageAsync(RoleName, result.Content, cancel);
            if (result.Content.Contains("Requirement analysis complete")) break;
            string? userInput = await _userInteraction.ReadUserMessageAsync(cancel);
            if (userInput is null) break;
            history.AddUserMessage(userInput);
            verifications.Add(new RequirementVerificationModel(result.Content, userInput));
        } while (true);
        return verifications;
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
                Ask questions to clarify the requirements. Maximum 2 questions can be asked. Ask questions one by one. Do not number the questions.
                When requirements are complete, respond with 'Requirement analysis complete'.
                Provide explanation of each question in the output. Explanation should be put in brackets and follow the question.
                Do not answer requests that are not related to project requirements analysis.
                """
        };
    }
}
