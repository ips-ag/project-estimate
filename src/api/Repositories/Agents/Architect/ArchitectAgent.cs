using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ProjectEstimate.Repositories.Agents.Architect.Models;
using ProjectEstimate.Repositories.Hubs;

#pragma warning disable SKEXP0010

namespace ProjectEstimate.Repositories.Agents.Architect;

internal class ArchitectAgent
{
    private const string RoleName = "Architect";
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletion;
    private readonly PromptExecutionSettings _executionSettings;
    private readonly IUserInteraction _userInteraction;

    public ArchitectAgent(
        Kernel kernel,
        IChatCompletionService chatCompletion,
        IUserInteraction userInteraction)
    {
        _kernel = kernel;
        _chatCompletion = chatCompletion;
        _userInteraction = userInteraction;
        _executionSettings = new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ResponseFormat = ChatResponseFormat.Json,
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

    public async ValueTask<EstimationModel?> EstimateAsync(ChatHistory history, CancellationToken cancel)
    {
        await _userInteraction.WriteAssistantMessageAsync(RoleName, "Identifying user stories and tasks ...", cancel);
        foreach (var message in history)
        {
            if (message.Content is null || message.Role == AuthorRole.System) continue;
            await _userInteraction.WriteAssistantMessageAsync(
                assistant: RoleName,
                message: $"Analyzing *{message.Content}*",
                logLevel: LogLevel.Debug,
                cancel: cancel);
        }
        await _userInteraction.WriteAssistantMessageAsync(RoleName, "Estimating tasks ...", cancel);
        var result = await _chatCompletion.GetChatMessageContentAsync(
            history,
            executionSettings: _executionSettings,
            kernel: _kernel,
            cancellationToken: cancel);
        await _userInteraction.WriteAssistantMessageAsync(RoleName, "Estimation complete", cancel);
        if (result.Content is null) return null;
        history.AddAssistantMessage(result.Content);
        await _userInteraction.WriteAssistantMessageAsync(
            assistant: RoleName,
            message: $"Estimation *{result.Content}*",
            logLevel: LogLevel.Debug,
            cancel: cancel);
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
