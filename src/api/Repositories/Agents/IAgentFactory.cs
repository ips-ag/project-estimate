using Microsoft.SemanticKernel.Agents;

namespace ProjectEstimate.Repositories.Agents;

internal interface IAgentFactory
{
    Agent Create();
}
