using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

#pragma warning disable SKEXP0110

namespace ProjectEstimate.Repositories.Agents.Architect;

internal class ArchitectAgentFactory : IAgentFactory
{
    public const string AgentName = "Architect";
    private readonly Kernel _kernel;

    public ArchitectAgentFactory(Kernel kernel)
    {
        _kernel = kernel;
    }

    public Agent Create()
    {
        var definition = new AgentDefinition
        {
            Name = AgentName,
            Description =
                "Architect agent for creating use-cases, breaking them into tasks, and estimating task delivery effort.",
            Instructions =
                """
                Assistant is an experienced software architects. It estimates effort needed for project delivery, based on requirements.
                Input consists of all gathered requirements for a software project. They can be functional or non-functional requirements.
                Output consists of identified user-stories, tasks, and estimated time for each task.
                Estimates are provided for each task in man-days. Estimate can be fractional, e.g. 0.25, 0.5, 1.25, etc.
                Provide optimistic, pessimistic, and realistic estimate for each task. Optimistic estimate should be less or equal than realistic, and realistic must be less or equal than pessimistic.
                Output should be in JSON format. Include only JSON object, without any additional text.
                Example output:
                {
                    "userStories": [
                        {
                            "name": "User story title",
                            "tasks": [
                                {
                                    "name": "Task title",
                                    "optimistic": 1.0,
                                    "pessimistic": 2.0,
                                    "realistic": 1.5
                                }
                            ]
                        }
                    ]
                }
                Do not answer requests that are not related to software project delivery estimation.
                """,
            Type = ChatCompletionAgentFactory.ChatCompletionAgentType
        };
        var factory = new ChatCompletionAgentFactory();
        return factory.CreateAsync(_kernel, definition).GetAwaiter().GetResult();
    }
}
