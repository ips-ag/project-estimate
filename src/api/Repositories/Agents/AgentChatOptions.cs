#pragma warning disable OPENAI001
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using ProjectEstimate.Repositories.Configuration;
using ReasoningEffort = ProjectEstimate.Repositories.Configuration.ReasoningEffort;

namespace ProjectEstimate.Repositories.Agents;

internal static class AgentChatOptions
{
    public static Func<IChatClient, object?> ReasoningEffortFactory(ReasoningEffort reasoningEffort)
    {
        return _ => new ChatCompletionOptions
        {
            ReasoningEffortLevel = new ChatReasoningEffortLevel(reasoningEffort.ToString().ToLowerInvariant())
        };
    }
}
