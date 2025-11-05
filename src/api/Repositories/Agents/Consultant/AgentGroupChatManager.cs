using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using ProjectEstimate.Repositories.Agents.Analyst;
using ProjectEstimate.Repositories.Agents.Developer;

namespace ProjectEstimate.Repositories.Agents.Consultant;

public class AgentGroupChatManager : GroupChatManager
{
    private readonly IReadOnlyList<AIAgent> _agents;
    private int _currentAgentIndex;

    public AgentGroupChatManager(IReadOnlyList<AIAgent> agents)
    {
        _agents = agents;
    }

    private ValueTask<AIAgent> GetAgentAsync(string agentName)
    {
        var agent = _agents.Single(a => agentName.Equals(a.Name));
        return new ValueTask<AIAgent>(agent);
    }

    protected override ValueTask<bool> ShouldTerminateAsync(
        IReadOnlyList<ChatMessage> history,
        CancellationToken cancellationToken = new())
    {
        var lastMessage = history.LastOrDefault();
        if (lastMessage is null)
        {
            return ValueTask.FromResult(false);
        }
        var lastAuthor = lastMessage.AuthorName ?? lastMessage.Role.Value;
        if (DeveloperAgentFactory.AgentName == lastAuthor)
        {
            return ValueTask.FromResult(true);
        }
        return ValueTask.FromResult(false);
    }

    protected override ValueTask<AIAgent> SelectNextAgentAsync(
        IReadOnlyList<ChatMessage> history,
        CancellationToken cancellationToken = new())
    {
        var lastMessage = history.LastOrDefault();
        if (lastMessage is not null)
        {
            var lastMessageAuthor = lastMessage.AuthorName ?? lastMessage.Role.Value;
            if ("user".Equals(lastMessageAuthor, StringComparison.OrdinalIgnoreCase))
            {
                return GetAgentAsync(AnalystAgentFactory.AgentName);
            }
        }
        // round-robin
        var nextAgent = _agents.Skip(_currentAgentIndex).First();
        _currentAgentIndex = (_currentAgentIndex + 1) % _agents.Count;
        return ValueTask.FromResult(nextAgent);
    }
}
