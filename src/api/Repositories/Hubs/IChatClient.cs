using ProjectEstimate.Repositories.Hubs.Models;

namespace ProjectEstimate.Repositories.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(string assistant, string message, MessageTypeModel type, bool final);
    Task<string?> AskQuestion(string assistant, string question);
}
