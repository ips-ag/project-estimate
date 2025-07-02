using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using ProjectEstimate.Domain;
using ProjectEstimate.Repositories.Agents.Analyst;
using ProjectEstimate.Repositories.Agents.Architect;
using ProjectEstimate.Repositories.Agents.Developer;
using ProjectEstimate.Repositories.Documents;
using ProjectEstimate.Repositories.Hubs;

namespace ProjectEstimate.Repositories.Agents.Consultant;

internal class ConsultantAgent
{
    private const string RoleName = "Consultant";
    private readonly Agent _analystAgent;
    private readonly Agent _architectAgent;
    private readonly Agent _developerAgent;
    private readonly IUserInteraction _userInteraction;
    private readonly IDocumentRepository _documentRepository;

    public ConsultantAgent(
        [FromKeyedServices(AnalystAgentFactory.AgentName)]
        Agent analystAgent,
        [FromKeyedServices(ArchitectAgentFactory.AgentName)]
        Agent architectAgent,
        [FromKeyedServices(DeveloperAgentFactory.AgentName)]
        Agent developerAgent,
        IUserInteraction userInteraction,
        IDocumentRepository documentRepository)
    {
        _analystAgent = analystAgent;
        _architectAgent = architectAgent;
        _developerAgent = developerAgent;
        _userInteraction = userInteraction;
        _documentRepository = documentRepository;
    }

    /// <summary>
    ///     Reads user input and writes agent output.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    public async ValueTask<string?> ExecuteAsync(ChatCompletionRequest request, CancellationToken cancellationToken)
    {
        string? userInput = request.Prompt;
        string? fileInput = await _documentRepository.ReadDocumentAsync(request.FileLocation, cancellationToken);
        // TODO: get history from repository
        ChatHistory history = [];
        var userMessage =
            $"""
             User prompt:
             \"\"\"{userInput}\"\"\"
             Additional context:
             \"\"\"{fileInput}\"\"\"
             """;
        history.AddUserMessage(userMessage);
        ChatHistoryAgentThread agentThread = new(history);
        foreach (var message in history)
        {
            if (message.Content is null || message.Role == AuthorRole.System) continue;
            await _userInteraction.WriteAssistantMessageAsync(
                assistant: RoleName,
                message: $"➜ **Analyst** Sending message *{message.Content}*",
                logLevel: LogLevel.Debug,
                cancel: cancellationToken);
        }
        await foreach (var item in _analystAgent.InvokeAsync(
                           thread: agentThread,
                           cancellationToken: cancellationToken))
        {
            string? message = item.Message.Content;
            if (message is not null) history.AddAssistantMessage(message);
        }
        foreach (var message in history)
        {
            if (message.Content is null || message.Role == AuthorRole.System) continue;
            await _userInteraction.WriteAssistantMessageAsync(
                assistant: RoleName,
                message: $"➜ **Architect** Sending message *{message.Content}*",
                logLevel: LogLevel.Debug,
                cancel: cancellationToken);
        }
        await foreach (var item in _architectAgent.InvokeAsync(
                           thread: agentThread,
                           cancellationToken: cancellationToken))
        {
            string? message = item.Message.Content;
            if (message is not null) history.AddAssistantMessage(message);
        }
        foreach (var message in history)
        {
            if (message.Content is null || message.Role == AuthorRole.System) continue;
            await _userInteraction.WriteAssistantMessageAsync(
                assistant: RoleName,
                message: $"➜ **Developer** Sending message *{message.Content}*",
                logLevel: LogLevel.Debug,
                cancel: cancellationToken);
        }
        await foreach (var item in _developerAgent.InvokeAsync(
                           thread: agentThread,
                           cancellationToken: cancellationToken))
        {
            string? message = item.Message.Content;
            if (message is not null) history.AddAssistantMessage(message);
        }
        return history.Last().Content;
    }

    public async ValueTask<string?> UploadFileAsync(UserFile file, CancellationToken cancel)
    {
        return await _documentRepository.CreateDocumentAsync(file, cancel);
    }
}
