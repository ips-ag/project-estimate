using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ProjectEstimate.Repositories.Agents.Analyst.Models;
using ProjectEstimate.Repositories.Hubs;

namespace ProjectEstimate.Repositories.Agents.Analyst;

internal class AnalystAgent
{
    private const string RoleName = "Analyst";
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletion;
    private readonly PromptExecutionSettings _executionSettings;
    private readonly IUserInteraction _userInteraction;

    public AnalystAgent(
        [FromKeyedServices("AnalystAgent")] Kernel kernel,
        IChatCompletionService chatCompletion,
        IUserInteraction userInteraction)
    {
        _kernel = kernel;
        _chatCompletion = chatCompletion;
        _userInteraction = userInteraction;
        _executionSettings = new AzureOpenAIPromptExecutionSettings
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

    public async ValueTask<List<RequirementVerificationModel>> VerifyRequirementsAsync(
        ChatHistory history,
        CancellationToken cancel)
    {
        List<RequirementVerificationModel> verifications = [];
        do
        {
            await _userInteraction.WriteAssistantMessageAsync(RoleName, "Analyzing requirements ...", cancel);
            foreach (var message in history)
            {
                if (message.Content is null || message.Role == AuthorRole.System) continue;
                await _userInteraction.WriteAssistantMessageAsync(
                    assistant: RoleName,
                    message: $"Analyzing *{message.Content}*",
                    logLevel: LogLevel.Debug,
                    cancel: cancel);
            }
            var result = await _chatCompletion.GetChatMessageContentAsync(
                history,
                executionSettings: _executionSettings,
                kernel: _kernel,
                cancellationToken: cancel);
            if (result.Content is null) break;
            history.AddAssistantMessage(result.Content);
            // await _userInteraction.WriteAssistantMessageAsync(RoleName, result.Content, cancel);
            if (result.Content.Contains("Requirement analysis complete"))
            {
                // ReSharper disable once RedundantJumpStatement
                break;
            }
            // TODO: User interaction
            // string? userInput = await _userInteraction.ReadUserMessageAsync(cancel);
            // if (userInput is null) break;
            // history.AddUserMessage(userInput);
            // verifications.Add(new RequirementVerificationModel(result.Content, userInput));
            break;
        } while (true);
        await _userInteraction.WriteAssistantMessageAsync(RoleName, "Requirement analysis complete", cancel);
        return verifications;
    }
}
