using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace ProjectEstimate.Repositories.Agents;

public class TestAgent
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletion;
    private readonly ChatHistory _history;
    private readonly PromptExecutionSettings _executionSettings;

    public TestAgent([FromKeyedServices("TestAgentKernel")] Kernel kernel, IChatCompletionService chatCompletion)
    {
        _kernel = kernel;
        _chatCompletion = chatCompletion;
        _history = [];
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

    public async Task<string?> CompleteAsync(string? input, CancellationToken cancel)
    {
        if (input is not null) _history.AddUserMessage(input);
        var response = await _chatCompletion.GetChatMessageContentAsync(_history, _executionSettings, _kernel, cancel);
        return response.Content;
    }
}
