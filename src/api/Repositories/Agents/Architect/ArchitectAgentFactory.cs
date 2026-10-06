using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ProjectEstimate.Repositories.Configuration;
using ReasoningEffort = ProjectEstimate.Repositories.Configuration.ReasoningEffort;

namespace ProjectEstimate.Repositories.Agents.Architect;

internal class ArchitectAgentFactory : IAgentFactory
{
    public const string AgentName = "Architect";
    private readonly IChatClient _chatClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ReasoningEffort _reasoningEffort;

    public ArchitectAgentFactory(IChatClient chatClient, ILoggerFactory loggerFactory, IServiceProvider serviceProvider,
        IOptionsMonitor<AgentSettings> agentSettingsMonitor)
    {
        _chatClient = chatClient;
        _loggerFactory = loggerFactory;
        _serviceProvider = serviceProvider;
        _reasoningEffort = agentSettingsMonitor.Get(AgentName).ReasoningEffort;
    }

    public AIAgent Create()
    {
        var options = new ChatClientAgentOptions
        {
            Name = AgentName,
            Description =
                "Architect agent for creating use-cases, breaking them into tasks, and estimating task delivery effort.",
            ChatOptions = new ChatOptions
            {
                RawRepresentationFactory = AgentChatOptions.ReasoningEffortFactory(_reasoningEffort),
                Instructions =
                    """
                    Assistant is an experienced software architects. It estimates effort needed for project delivery, based on requirements.
                    Input consists of all gathered requirements for a software project. They can be functional or non-functional requirements.
                    Output consists of identified user-stories, and tasks.
                    Output should be in Markdown format, specifically a Markdown table. Include only Markdown table, without any additional text.
                    Table should have the following columns: User Story, Task.
                    Example output format:

                    |User Story|Task|
                    |:---|:---|
                    |Basic Application Structure|Create .NET console application|
                    |Basic Application Structure|Setup project dependencies|
                    |Input Handling|Implement user input logic|
                    |Input Handling|Validate user input|
                    |Business Logic|Implement core application logic|
                    |Output Handling|Display output to user|
                    |Testing|Write unit tests for input logic|
                    |Testing|Write unit tests for business logic|
                    |Documentation|Create user manual|
                    |Documentation|Document code and methods|

                    Do not answer requests that are not related to software project delivery estimation.
                    """
            }
        };
        return _chatClient.AsAIAgent(options: options, loggerFactory: _loggerFactory, services: _serviceProvider);
    }
}
