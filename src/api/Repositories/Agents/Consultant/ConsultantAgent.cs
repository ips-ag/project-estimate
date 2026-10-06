using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using ProjectEstimate.Domain;
using ProjectEstimate.Repositories.Agents.Analyst;
using ProjectEstimate.Repositories.Agents.Architect;
using ProjectEstimate.Repositories.Agents.Developer;
using ProjectEstimate.Repositories.Agents.Tools;
using ProjectEstimate.Repositories.Documents;
using ProjectEstimate.Repositories.Hubs;

namespace ProjectEstimate.Repositories.Agents.Consultant;

internal class ConsultantAgent
{
    private readonly AIAgent _analystAgent;
    private readonly AIAgent _architectAgent;
    private readonly AIAgent _developerAgent;
    private readonly IUserInteraction _userInteraction;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<ConsultantAgent> _logger;

    public ConsultantAgent(
        [FromKeyedServices(AnalystAgentFactory.AgentName)]
        AIAgent analystAgent,
        [FromKeyedServices(ArchitectAgentFactory.AgentName)]
        AIAgent architectAgent,
        [FromKeyedServices(DeveloperAgentFactory.AgentName)]
        AIAgent developerAgent,
        IUserInteraction userInteraction,
        IDocumentRepository documentRepository,
        ILogger<ConsultantAgent> logger)
    {
        _analystAgent = analystAgent;
        _architectAgent = architectAgent;
        _developerAgent = developerAgent;
        _userInteraction = userInteraction;
        _documentRepository = documentRepository;
        _logger = logger;
    }

    /// <summary>
    ///     Reads user input and writes agent output.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    public async ValueTask ExecuteAsync(ChatCompletionRequest request, CancellationToken cancellationToken)
    {
        string? userInput = request.Prompt;
        string? fileInput = await _documentRepository.ReadDocumentAsync(request.FileLocation, cancellationToken);
        var userMessage =
            $"""
             User prompt:
             \"\"\"{userInput}\"\"\"
             Additional context:
             \"\"\"{fileInput}\"\"\"
             """;
        // TODO: get history from repository
        List<ChatMessage> history = [new(ChatRole.User, userMessage)];
        var workflow =  AgentWorkflowBuilder
            .CreateGroupChatBuilderWith(agents => new AgentGroupChatManager(agents))
            .AddParticipants(_analystAgent, _architectAgent, _developerAgent)
            .Build();
        await using var run = await InProcessExecution.RunStreamingAsync(
            workflow: workflow,
            input: history,
            cancellationToken: cancellationToken);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
        string? lastExecutorId = null;
        var assistant = "Assistant";
        StringBuilder messageBuilder = new();
        await foreach (var evt in run.WatchStreamAsync(cancellationToken).ConfigureAwait(false))
        {
            switch (evt)
            {
                // agent processing finished
                case AgentResponseUpdateEvent e:
                {
                    string tokens = e.Update.Text;
                    if (string.IsNullOrEmpty(tokens) && e.Update.Contents.Count == 0) continue;
                    if (e.ExecutorId != lastExecutorId)
                    {
                        if (messageBuilder.Length > 0)
                        {
                            var message = messageBuilder.ToString();
                            await _userInteraction.MessageOutputAsync(
                                assistant: assistant,
                                message: message,
                                conversationEnd: false,
                                cancel: cancellationToken);
                        }
                        lastExecutorId = e.ExecutorId;
                        messageBuilder.Clear();
                    }
                    assistant = e.Update.AuthorName ?? e.Update.Role?.Value ?? "Assistant";
                    messageBuilder.Append(tokens);
                    break;
                }
                case RequestInfoEvent info:
                {
                    if (messageBuilder.Length > 0)
                    {
                        await _userInteraction.MessageOutputAsync(
                            assistant: assistant,
                            message: messageBuilder.ToString(),
                            conversationEnd: false,
                            cancel: cancellationToken);
                        messageBuilder.Clear();
                    }
                    var response = await HandleRequestAsync(info.Request, assistant, cancellationToken);
                    await run.SendResponseAsync(response);
                    break;
                }
                // conversation end
                case WorkflowOutputEvent output:
                {
                    var chatMessages = output.As<List<ChatMessage>>()!;
                    var lastMessage = chatMessages.Last();
                    string message = lastMessage.Text;
                    assistant = lastMessage.AuthorName ?? lastMessage.Role.Value;
                    await _userInteraction.MessageOutputAsync(
                        assistant: assistant,
                        message: message,
                        conversationEnd: true,
                        cancel: cancellationToken);
                    break;
                }
                // workflow error
                case WorkflowErrorEvent error:
                {
                    await _userInteraction.MessageOutputAsync(
                        assistant: assistant,
                        message: "Encountered an error",
                        conversationEnd: true,
                        cancel: cancellationToken);
                    var ex = error.Data as Exception;
                    _logger.LogWarning(ex, "Workflow error");
                    break;
                }
            }
        }
    }

    private async ValueTask<ExternalResponse> HandleRequestAsync(
        ExternalRequest request,
        string assistant,
        CancellationToken cancellationToken)
    {
        if (request.TryGetDataAs(out FunctionCallContent? call))
        {
            if (AskUserTool.Name != call.Name)
            {
                _logger.LogWarning("Unsupported function call request {FunctionName}", call.Name);
                return request.CreateResponse(
                    new FunctionResultContent(call.CallId, $"Tool '{call.Name}' is not available."));
            }
            object? questionArgument = null;
            call.Arguments?.TryGetValue(AskUserTool.QuestionParameter, out questionArgument);
            string question = questionArgument?.ToString() ?? string.Empty;
            // asking agent is the last one which streamed output, fall back to the port id: {executorId}_FunctionCall
            string asker = assistant != "Assistant" ? assistant : request.PortInfo.PortId.Split('_')[0];
            await _userInteraction.MessageOutputAsync(
                assistant: asker,
                message: question,
                conversationEnd: false,
                cancel: cancellationToken);
            string? answer = await _userInteraction.GetAnswerAsync(cancel: cancellationToken);
            return request.CreateResponse(new FunctionResultContent(call.CallId, answer ?? "No answer provided"));
        }
        if (request.TryGetDataAs(out ToolApprovalRequestContent? approval))
        {
            _logger.LogWarning("Tool approval is not supported, denying request");
            return request.CreateResponse(approval.CreateResponse(approved: false, reason: "Approval is not supported."));
        }
        throw new InvalidOperationException($"Unsupported workflow request from port {request.PortInfo.PortId}");
    }

    public async ValueTask<string?> UploadFileAsync(UserFile file, CancellationToken cancel)
    {
        return await _documentRepository.CreateDocumentAsync(file, cancel);
    }
}
