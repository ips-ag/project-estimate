using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ProjectEstimate.Repositories.Agents.Tools;
using ProjectEstimate.Repositories.Configuration;
using ReasoningEffort = ProjectEstimate.Repositories.Configuration.ReasoningEffort;

namespace ProjectEstimate.Repositories.Agents.Analyst;

internal class AnalystAgentFactory : IAgentFactory
{
    public const string AgentName = "Analyst";
    private readonly IChatClient _chatClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ReasoningEffort _reasoningEffort;

    public AnalystAgentFactory(IChatClient chatClient, ILoggerFactory loggerFactory, IServiceProvider serviceProvider,
        IOptionsMonitor<AgentSettings> agentSettingsMonitor)
    {
        _chatClient = chatClient;
        _loggerFactory = loggerFactory;
        _serviceProvider = serviceProvider;
        _reasoningEffort = agentSettingsMonitor.Get(AgentName).ReasoningEffort;
    }

    public AIAgent Create()
    {
        var instructions =
            $"""
             You are an experienced business analysts. You analyze and verify project requirements.
             Input consists of all gathered requirements for a software project. They can be functional or non-functional requirements.
             You can ask questions to clarify the requirements.
             To ask a question, call the '{AskUserTool.Name}' tool. Never ask questions in plain text.
             Only ask one question per conversation round. In total, ask maximum of two questions. Do not number the questions.
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
             """;
        var description = "Analyst agent for verifying project requirements.";
        var options = new ChatClientAgentOptions
        {
            Name = AgentName,
            Description = description,
            ChatOptions = new ChatOptions
            {
                RawRepresentationFactory = AgentChatOptions.ReasoningEffortFactory(_reasoningEffort),
                Instructions = instructions,
                Tools = [AskUserTool.Declaration]
            }
        };
        return _chatClient.AsAIAgent(options: options, loggerFactory: _loggerFactory, services: _serviceProvider);
    }
}
