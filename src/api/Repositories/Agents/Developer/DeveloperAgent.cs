using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ProjectEstimate.Repositories.Agents.Developer.Models;
#pragma warning disable SKEXP0010

namespace ProjectEstimate.Repositories.Agents.Developer;

internal class DeveloperAgent
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletion;
    private readonly PromptExecutionSettings _executionSettings;

    public DeveloperAgent([FromKeyedServices("DeveloperAgent")] Kernel kernel, IChatCompletionService chatCompletion)
    {
        _kernel = kernel;
        _chatCompletion = chatCompletion;
        _executionSettings = new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ResponseFormat = ChatResponseFormat.Json,
            ChatSystemPrompt =
                """
                Assistant is an experienced software developer. It validates and corrects effort estimates for project delivery, based on existing requirements and previous estimates.
                Input consists of all gathered requirements and previous estimates for a software project. They can be functional or non-functional requirements. Input is found in chat history.
                Output consists of corrected estimates for development task. Do not add any pseudo code or samples, only estimates according to output format definition.
                Estimates are provided for each task in man-days. Estimate can be fractional, e.g. 0.25, 0.5, 1.25, etc.
                Validate optimistic, pessimistic, and realistic estimate for each task. Correct if needed, based on software developer experience. If estimates are modified, provide correction reason, otherwise do not include it.
                Output should be in JSON format. Include only JSON object, without any additional text.
                Example output:
                {
                    "userStories": [
                        {
                            "name": "User story title",
                            "tasks": [
                                {
                                    "name": "Task title",
                                    "correctionReason": "Optimistic estimate was too low",
                                    "optimistic": 1.0,
                                    "pessimistic": 2.0,
                                    "realistic": 1.5
                                }
                            ]
                        }
                    ]
                }
                Do not answer requests that are not related to software project delivery estimation validation.
                """
        };
    }

    public async ValueTask<EstimationModel?> ValidateEstimatesAsync(ChatHistory history, CancellationToken cancel)
    {
        var result = await _chatCompletion.GetChatMessageContentAsync(
            history,
            executionSettings: _executionSettings,
            kernel: _kernel,
            cancellationToken: cancel);
        //await _userInteraction.WriteAssistantMessageAsync(RoleName, "Estimation validation complete", cancel);
        if (result.Content is null) return null;
        history.AddAssistantMessage(result.Content);
        try
        {
            int start = result.Content.IndexOf('{');
            int end = result.Content.LastIndexOf('}');
            if (start == -1 || end == -1) return null;
            string json = result.Content.Substring(start, end - start + 1);
            return JsonSerializer.Deserialize<EstimationModel>(json);
        }
        catch (JsonException)
        {
            // await _userInteraction.WriteAssistantMessageAsync(RoleName, result.Content, cancel);
            return null;
        }
    }
}
