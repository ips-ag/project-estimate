using Microsoft.AspNetCore.SignalR;

namespace ProjectEstimate.Repositories.Hubs;

internal class ChatHub : Hub<IChatClient>
{
}
