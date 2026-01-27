using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using ProjectEstimate.Domain;
using ProjectEstimate.Repositories.Agents.Analyst;
using ProjectEstimate.Repositories.Agents.Architect;
using ProjectEstimate.Repositories.Agents.Developer;
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
        // async ValueTask ResponseCallback(ChatMessageContent message)
        // {
        //     history.Add(message);
        //     string assistant = message.AuthorName ?? message.Role.Label;
        //     string content = message.Content?.Trim() ?? string.Empty;
        //     if (DeveloperAgentFactory.AgentName == assistant)
        //     {
        //         await _userInteraction.MessageOutputAsync(
        //             assistant: assistant,
        //             message: content,
        //             conversationEnd: true,
        //             cancel: cancellationToken);
        //         return;
        //     }
        //     bool isReasoning = AnalystAgentFactory.AgentName != assistant;
        //     if (isReasoning)
        //     {
        //         await _userInteraction.ReasoningOutputAsync(
        //             assistant: assistant,
        //             message: content,
        //             cancel: cancellationToken);
        //     }
        //     else
        //     {
        //         await _userInteraction.MessageOutputAsync(
        //             assistant: assistant,
        //             message: content,
        //             conversationEnd: false,
        //             cancel: cancellationToken);
        //     }
        // }
        //
        // async ValueTask<ChatMessageContent> InteractiveCallback()
        // {
        //     var lastMessage = history.LastOrDefault();
        //     string? question = lastMessage?.Content;
        //     string? answer = null;
        //     if (question is not null)
        //     {
        //         answer = await _userInteraction.GetAnswerAsync(cancel: cancellationToken);
        //     }
        //     ChatMessageContent input = new(role: AuthorRole.User, content: answer)
        //     {
        //         AuthorName = AuthorRole.User.Label
        //     };
        //     return input;
        // }

        //AgentGroupChatManager chatManager = new(history) { InteractiveCallback = InteractiveCallback };
        RequestPort answerPort = RequestPort.Create<string, string>("UserAnswer");
        var workflow =  AgentWorkflowBuilder
            .CreateGroupChatBuilderWith(agents => new AgentGroupChatManager(agents))
            .AddParticipants(_analystAgent, _architectAgent, _developerAgent)
            .Build();
        // GroupChatOrchestration orchestration = new(chatManager, _analystAgent, _architectAgent, _developerAgent)
        // {
        //     LoggerFactory = _loggerFactory, ResponseCallback = ResponseCallback
        // };
        await using var run = await InProcessExecution.StreamAsync(
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
                    var question = info.Request.DataAs<string>();
                    string? answer = await _userInteraction.GetAnswerAsync(cancel: cancellationToken);
                    var response = info.Request.CreateResponse(answer);
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
        // InProcessRuntime runtime = new();
        // await runtime.StartAsync(cancellationToken);
        // var result = await orchestration.InvokeAsync(userMessage, runtime, cancellationToken);
        // string output = await result.GetValueAsync(cancellationToken: cancellationToken);
        // await runtime.RunUntilIdleAsync();
    }

    public async ValueTask<string?> UploadFileAsync(UserFile file, CancellationToken cancel)
    {
        return await _documentRepository.CreateDocumentAsync(file, cancel);
    }
}
