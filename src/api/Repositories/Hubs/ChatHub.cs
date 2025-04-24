using Microsoft.AspNetCore.SignalR;

namespace ProjectEstimate.Repositories.Hubs;

internal class ChatHub : Hub<IChatClient>
{
    // [HubMethodName("ReceiveMessage")]
    // public async Task ReceiveMessage(string assistant, string message)
    // {
    //     await Clients.All.ReceiveMessage(assistant, message);
    // }
}
