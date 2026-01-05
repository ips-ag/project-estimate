using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace ProjectEstimate.Repositories.Agents.Analyst;

internal class AnalystAgentFactory : IAgentFactory
{
    public const string AgentName = "Analyst";
    private readonly IChatClient _chatClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _serviceProvider;

    public AnalystAgentFactory(IChatClient chatClient, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    {
        _chatClient = chatClient;
        _loggerFactory = loggerFactory;
        _serviceProvider = serviceProvider;
    }

    public AIAgent Create()
    {
        var instructions =
            "You are an experienced business analysts. Just answer 'Requirement analysis complete' to any request and nothing else.";
        var description = "Analyst agent for verifying project requirements.";
        var options = new ChatClientAgentOptions
        {
            Name = AgentName,
            Description = description,
            ChatOptions = new ChatOptions
            {
                Instructions = instructions
            }
            // """
            // You are an experienced business analysts. You analyze and verify project requirements.
            // Input consists of all gathered requirements for a software project. They can be functional or non-functional requirements.
            // You can ask questions to clarify the requirements.
            // Only ask one question per conversation round. In total, ask maximum of two questions. Do not number the questions.
            // Provide explanation for each question. Explanation should be put in brackets and follow the question.
            // End each question message with a question mark. Do not put question mark after the question itself, but at the end of a message. Use format '<question> (<explanation>)?'.
            // Use questions to clarify the requirements with respect to following aspects. Ignore aspect if already provided.
            // * technical constraints (platforms, languages, frameworks, etc.)
            // * number of users (concurrent and total)
            // * use-case completeness (what users can do with the system, all inputs and outputs)
            // * integration with other business systems (e.g., ERP, CRM, billing, customer API, etc.)
            // * security requirements (e.g., authentication, authorization, data protection, etc.)
            // * compliance requirements (e.g., GDPR, HIPAA, etc.)
            // When requirements analysis is complete, and all questions are answered, say 'Requirement analysis complete'.
            // Do not answer requests that are not related to project requirements analysis.
            // """
        };
        return _chatClient.CreateAIAgent(options: options, loggerFactory: _loggerFactory, services: _serviceProvider);
    }
}
