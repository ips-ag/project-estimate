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
                You are an experienced business analysts. You analyze and verify project requirements.
                Input consists of all gathered requirements for a software project. They can be functional or non-functional requirements.
                You can ask questions to clarify the requirements.
                Only ask one question per conversation round. In total, ask maximum of 3 questions. Do not number the questions.
                Provide explanation for each question. Explanation should be put in brackets and follow the question.
                Use questions to clarify the requirements with respect to following aspects. Ignore aspect if already provided.
                * technical constraints (platforms, languages, frameworks, etc.)
                * number of users (concurrent and total)
                * use-case completeness (what users can do with the system, all inputs and outputs)
                * integration with other business systems (e.g., ERP, CRM, billing, customer API, etc.)
                * security requirements (e.g., authentication, authorization, data protection, etc.)
                * compliance requirements (e.g., GDPR, HIPAA, etc.)
                When requirements analysis is complete, and all questions are answered, say 'Requirement analysis complete'.
                Do not answer requests that are not related to project requirements analysis.
                """,
            Type = ChatCompletionAgentFactory.ChatCompletionAgentType
        };
        var factory = new ChatCompletionAgentFactory();
        return factory.CreateAsync(_kernel, definition).GetAwaiter().GetResult();
    }
}
