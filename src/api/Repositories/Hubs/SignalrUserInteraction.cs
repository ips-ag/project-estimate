using Microsoft.AspNetCore.SignalR;
using ProjectEstimate.Application.Request.Context;
using ProjectEstimate.Repositories.Hubs.Converters;

namespace ProjectEstimate.Repositories.Hubs;

internal class SignalrUserInteraction : IUserInteraction
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly IRequestContextAccessor _requestContextAccessor;
    private readonly LogLevelConverter _logLevelConverter;

    public SignalrUserInteraction(
        IHubContext<ChatHub, IChatClient> hubContext,
        IRequestContextAccessor requestContextAccessor,
        LogLevelConverter logLevelConverter)
    {
        _hubContext = hubContext;
        _requestContextAccessor = requestContextAccessor;
        _logLevelConverter = logLevelConverter;
    }

    public async ValueTask WriteAssistantMessageAsync(
        string assistant,
        string message,
        CancellationToken cancel,
        LogLevel logLevel = LogLevel.Information)
    {
        string? connectionId = _requestContextAccessor.Context?.ConnectionId;
        if (connectionId is null) return;
        var logLevelModel = _logLevelConverter.ToModel(logLevel);
        await _hubContext.Clients.Client(connectionId)
            .ReceiveMessage(assistant, message, logLevelModel)
            .WaitAsync(cancel);
    }

    public async ValueTask<string?> AskQuestionAsync(string assistant, string question, CancellationToken cancel)
    {
        string? connectionId = _requestContextAccessor.Context?.ConnectionId;
        if (connectionId is null) return null;
        string? answer = await _hubContext.Clients.Client(connectionId)
            .AskQuestion(assistant, question)
            .WaitAsync(cancel);
        return answer;
    }
}
