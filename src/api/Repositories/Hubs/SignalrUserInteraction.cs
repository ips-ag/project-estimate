using Microsoft.AspNetCore.SignalR;
using ProjectEstimate.Application.Request.Context;

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

    public async ValueTask WriteAssistantMessageAsync(string assistant, string message, CancellationToken cancel)
    {
        string? connectionId = _requestContextAccessor.Context?.ConnectionId;
        if (connectionId is null) return;
        await _hubContext.Clients.Client(connectionId).ReceiveMessage(assistant, message);
    }
}
