using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.ChatCompletion;
using ProjectEstimate.Repositories.Agents.Analyst;
using ProjectEstimate.Repositories.Agents.Developer;

#pragma warning disable SKEXP0001

#pragma warning disable SKEXP0110

namespace ProjectEstimate.Repositories.Agents.Consultant;

public class AgentGroupChatManager : GroupChatManager
{
    private int _currentAgentIndex;

    public override ValueTask<GroupChatManagerResult<string>> FilterResults(
        ChatHistory history,
        CancellationToken cancellationToken = new())
    {
        GroupChatManagerResult<string> result = new(history.LastOrDefault()?.Content ?? string.Empty)
        {
            Reason = "Default result filter provides the final chat message."
        };
        return ValueTask.FromResult(result);
    }

    public override ValueTask<GroupChatManagerResult<string>> SelectNextAgent(
        ChatHistory history,
        GroupChatTeam team,
        CancellationToken cancellationToken = new())
    {
        // round-robin
        string nextAgent = team.Skip(_currentAgentIndex).First().Key;
        _currentAgentIndex = (_currentAgentIndex + 1) % team.Count;
        GroupChatManagerResult<string> result = new(nextAgent)
        {
            Reason = $"Selected agent at index: {_currentAgentIndex}"
        };
        return ValueTask.FromResult(result);
    }

    public override ValueTask<GroupChatManagerResult<bool>> ShouldRequestUserInput(
        ChatHistory history,
        CancellationToken cancellationToken = new())
    {
        GroupChatManagerResult<bool> result;
        var lastMessage = history.LastOrDefault();
        if (lastMessage is null)
        {
            result = new GroupChatManagerResult<bool>(false) { Reason = "Conversation not started." };
            return ValueTask.FromResult(result);
        }
        string lastAuthor = lastMessage.AuthorName ?? lastMessage.Role.Label;
        if (AnalystAgentFactory.AgentName == lastAuthor)
        {
            result = new GroupChatManagerResult<bool>(true) { Reason = "Question asked by analyst." };
            return ValueTask.FromResult(result);
        }
        result = new GroupChatManagerResult<bool>(false) { Reason = "User input was not requested." };
        return ValueTask.FromResult(result);
    }

    public override ValueTask<GroupChatManagerResult<bool>> ShouldTerminate(
        ChatHistory history,
        CancellationToken cancellationToken = new())
    {
        GroupChatManagerResult<bool> result;
        var lastMessage = history.LastOrDefault();
        if (lastMessage is null)
        {
            result = new GroupChatManagerResult<bool>(false) { Reason = "Conversation not started." };
            return ValueTask.FromResult(result);
        }
        string lastAuthor = lastMessage.AuthorName ?? lastMessage.Role.Label;
        if (DeveloperAgentFactory.AgentName == lastAuthor)
        {
            result = new GroupChatManagerResult<bool>(true) { Reason = "Agent sequence completed." };
            return ValueTask.FromResult(result);
        }
        result = new GroupChatManagerResult<bool>(false) { Reason = "Termination reason not reached." };
        return ValueTask.FromResult(result);
    }
}
