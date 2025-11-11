using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace ProjectEstimate.Repositories.Agents.Developer;

internal class DeveloperAgentFactory : IAgentFactory
{
    public const string AgentName = "Developer";
    private readonly IChatClient _chatClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _serviceProvider;

    public DeveloperAgentFactory(IChatClient chatClient, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    {
        _chatClient = chatClient;
        _loggerFactory = loggerFactory;
        _serviceProvider = serviceProvider;
    }

    public AIAgent Create()
    {
        var options = new ChatClientAgentOptions
        {
            Name = AgentName,
            Description =
                "Developer agent for validating and correcting effort estimates for software project delivery.",
            Instructions =
                """
                You are an experienced software developer. You validate and create task estimates for project delivery, based on existing requirements, user-stories, and tasks.
                Input consists of all gathered requirements and breakdown of requirements into user-stories and tasks. Requirements can be functional or non-functional. Input is found in chat history.
                Output consists of estimates for development tasks. Do not add any pseudo code or samples, only estimates according to output format definition.
                Estimates are provided for each task in man-days. Estimate can be fractional, e.g. 0.25, 0.5, 1.25, 2, 3, 5.25, etc.
                Validate optimistic, pessimistic, and realistic estimate for each task.
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

                Do not answer requests that are not related to software project delivery estimation validation.
                """
        };
        return _chatClient.CreateAIAgent(options: options, loggerFactory: _loggerFactory, services: _serviceProvider);
    }
}
