namespace ProjectEstimate.Repositories.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(string assistant, string message);
}
