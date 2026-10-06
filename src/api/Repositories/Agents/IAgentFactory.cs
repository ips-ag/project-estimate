using Microsoft.Agents.AI;

namespace ProjectEstimate.Repositories.Agents;

internal interface IAgentFactory
{
    AIAgent Create();
}
