using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

#pragma warning disable SKEXP0110

namespace ProjectEstimate.Repositories.Agents.Analyst;

internal class AnalystAgentFactory : IAgentFactory
{
    public const string AgentName = "Analyst";
    private readonly Kernel _kernel;

    public AnalystAgentFactory(Kernel kernel)
    {
        _kernel = kernel;
    }

    public Agent Create()
    {
        var definition = new AgentDefinition
        {
            Name = AgentName,
            Description = "Analyst agent for verifying project requirements.",
            Metadata = new AgentMetadata { Authors = [AgentName] },
            Instructions =
                """
                Assistant is a business analysts. It verifies project requirements.
                Input consists of all gathered requirements for a software project. They can be functional or non-functional requirements.
                Ask question to clarify the requirements. Maximum 1 question can be asked. Do not number the questions.
                Make sure to clarify the requirements with respect to following aspects. Ignore aspect if already provided.
                * technical constraints (platforms, languages, frameworks, etc.)
                * number of users (concurrent and total)
                * use-case completeness (what users can do with the system, all inputs and outputs)
                * integration with other business systems (e.g., ERP, CRM, billing, customer API, etc.)
                When requirements gathering is complete, respond with 'Requirement analysis complete'.
                Provide explanation of each question in the output. Explanation should be put in brackets and follow the question.
                Do not answer requests that are not related to project requirements analysis.
                """,
            Type = ChatCompletionAgentFactory.ChatCompletionAgentType
        };
        var factory = new ChatCompletionAgentFactory();
        return factory.CreateAsync(_kernel, definition).GetAwaiter().GetResult();
    }
}
