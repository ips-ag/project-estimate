using System.Threading.Channels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProjectEstimate.Repositories.Hubs.Models;

namespace ProjectEstimate.Repositories.Hubs;

[Authorize]
internal class ChatHub : Hub<IChatClient>
{
    private readonly Channel<ChatCompletionRequestModel> _workQueue;

    public ChatHub(Channel<ChatCompletionRequestModel> workQueue)
    {
        _workQueue = workQueue;
    }

    /// <summary>
    /// Receives a message from the client
    /// </summary>
    /// <param name="prompt">User textual prompt</param>
    /// <param name="fileLocation">Location of a file uploaded by the user</param>
    [HubMethodName("sendMessage")]
    public async Task SendMessage(string? prompt, string? fileLocation)
    {
        ChatCompletionRequestModel requestModel = new()
        {
            Prompt = prompt, FileLocation = fileLocation, ConnectionId = Context.ConnectionId
        };
        await _workQueue.Writer.WriteAsync(requestModel, Context.ConnectionAborted);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // TODO: stop any active agent chat session
        return base.OnDisconnectedAsync(exception);
    }
}
