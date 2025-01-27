namespace ProjectEstimate.Agents;

internal class ConsoleInteraction : IUserInteraction
{
    public async ValueTask<string?> ReadUserMessageAsync(CancellationToken cancel)
    {
        await Console.Out.WriteAsync("User > ");
        return await Console.In.ReadLineAsync(cancel);
    }

    public ValueTask WriteAssistantMessageAsync(string message, CancellationToken cancel)
    {
        Console.WriteLine($"Assistant > {message}");
        return ValueTask.CompletedTask;
    }
}
