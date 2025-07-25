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
            Metadata = new AgentMetadata { Authors = [AgentName] },
            Instructions =
                """
                Assistant is an experienced software architects. It estimates effort needed for project delivery, based on requirements.
                Input consists of all gathered requirements for a software project. They can be functional or non-functional requirements.
                Output consists of identified user-stories, tasks, and estimated time for each task.
                Estimates are provided for each task in man-days. Estimate can be fractional, e.g. 0.25, 0.5, 1.25, etc.
                Provide optimistic, pessimistic, and realistic estimate for each task. Optimistic estimate should be less or equal than realistic, and realistic must be less or equal than pessimistic.
                Output should be in Markdown format, specifically a Markdown table. Include only Markdown table, without any additional text.
                Table should have the following columns: User Story, Task, Optimistic, Realistic, Pessimistic, Effort.
                Effort for each row should be calculated as (optimistic+4*realistic+pessimistic)/6.
                Total effort should be calculated as sum of all tasks' efforts and displayed in the last row of the table.
                Example output format:

                |User Story|Task|Optimistic|Realistic|Pessimistic|Effort|
                |:---|:---|:---|:---|:---|:---|:---|
                |Basic Application Structure|Create .NET console application|0.5|0.75|1|0.75|
                |Basic Application Structure|Setup project dependencies|0.25|0.375|0.5|0.375|
                |Input Handling|Implement user input logic|1|1.5|2|1.5|
                |Input Handling|Validate user input|0.5|0.75|1|0.75|
                |Business Logic|Implement core application logic|2|3|4|3|
                |Output Handling|Display output to user|0.5|0.75|1|0.75|
                |Testing|Write unit tests for input logic|0.5|0.75|1|0.75|
                |Testing|Write unit tests for business logic|0.75|1|1.5|1.0417|
                |Documentation|Create user manual|1|1.5|2|1.5|
                |Documentation|Document code and methods|0.5|0.75|1|0.75|
                |__Total effort__|||||__11.1667__|

                Do not answer requests that are not related to software project delivery estimation.
                """,
            Type = ChatCompletionAgentFactory.ChatCompletionAgentType
        };
        var factory = new ChatCompletionAgentFactory();
        return factory.CreateAsync(_kernel, definition).GetAwaiter().GetResult();
    }
}
