using Microsoft.AspNetCore.SignalR;
using ProjectEstimate.Application.Request.Context;
using ProjectEstimate.Repositories.Hubs.Models;

namespace ProjectEstimate.Repositories.Hubs;

internal class SignalrUserInteraction : IUserInteraction
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly IRequestContextAccessor _requestContextAccessor;

    public SignalrUserInteraction(
        IHubContext<ChatHub, IChatClient> hubContext,
        IRequestContextAccessor requestContextAccessor)
    {
        _hubContext = hubContext;
        _requestContextAccessor = requestContextAccessor;
    }

    public async ValueTask MessageOutputAsync(
        string assistant,
        string message,
        bool conversationEnd = false,
        CancellationToken cancel = default)
    {
        string? connectionId = _requestContextAccessor.Context?.ConnectionId;
        if (connectionId is null) return;
        await _hubContext.Clients.Client(connectionId)
            .ReceiveMessage(assistant, message, MessageTypeModel.Message, conversationEnd)
            .WaitAsync(cancel);
    }

    public async ValueTask ReasoningOutputAsync(string assistant, string message, CancellationToken cancel = default)
    {
        string? connectionId = _requestContextAccessor.Context?.ConnectionId;
        if (connectionId is null) return;
        await _hubContext.Clients.Client(connectionId)
            .ReceiveMessage(assistant, message, MessageTypeModel.Reasoning, false)
            .WaitAsync(cancel);
    }

    public async ValueTask<string?> GetAnswerAsync(CancellationToken cancel)
    {
        string? connectionId = _requestContextAccessor.Context?.ConnectionId;
        if (connectionId is null) return null;
        string? answer = await _hubContext.Clients.Client(connectionId)
            .GetUserInput()
            .WaitAsync(cancel);
        return answer;
    }
}
