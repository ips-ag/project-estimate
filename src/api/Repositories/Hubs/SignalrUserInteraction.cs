using Microsoft.AspNetCore.SignalR;

namespace ProjectEstimate.Repositories.Hubs;

internal class SignalrUserInteraction : IUserInteraction
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;

    public SignalrUserInteraction(IHubContext<ChatHub, IChatClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async ValueTask WriteAssistantMessageAsync(string assistant, string message, CancellationToken cancel)
    {
        await _hubContext.Clients.All.ReceiveMessage(assistant, message);
    }
}
