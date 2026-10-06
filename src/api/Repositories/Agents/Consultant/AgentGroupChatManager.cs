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

    protected override ValueTask<IEnumerable<ChatMessage>> UpdateHistoryAsync(
        IReadOnlyList<ChatMessage> history,
        CancellationToken cancellationToken = new())
    {
        List<ChatMessage> filtered = [];
        foreach (var message in history)
        {
            var contents = message.Contents
                .Where(c => c is not FunctionCallContent and not FunctionResultContent)
                .ToList();
            if (contents.Count == 0) continue;
            if (contents.Count == message.Contents.Count)
            {
                filtered.Add(message);
                continue;
            }
            var clone = message.Clone();
            clone.Contents = contents;
            filtered.Add(clone);
        }
        return ValueTask.FromResult<IEnumerable<ChatMessage>>(filtered);
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
                var analystIndex = _agents.ToList().FindIndex(a => AnalystAgentFactory.AgentName.Equals(a.Name));
                _currentAgentIndex = (analystIndex + 1) % _agents.Count;
                return GetAgentAsync(AnalystAgentFactory.AgentName);
            }
        }
        // round-robin
        var nextAgent = _agents.Skip(_currentAgentIndex).First();
        _currentAgentIndex = (_currentAgentIndex + 1) % _agents.Count;
        return ValueTask.FromResult(nextAgent);
    }
}
