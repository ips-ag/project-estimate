using System.Text;

namespace ProjectEstimate.Agents;

internal class ConsoleInteraction : IUserInteraction
{
    public async ValueTask<string?> ReadUserMessageAsync(CancellationToken cancel)
    {
        await Console.Out.WriteAsync("User > ");
        return await Console.In.ReadLineAsync(cancel);
    }

    public async ValueTask<string?> ReadMultilineUserMessageAsync(CancellationToken cancel)
    {
        await Console.Out.WriteAsync("User (double Enter to submit)> ");
        var stringBuilder = new StringBuilder();
        string? line;
        do
        {
            line = await Console.In.ReadLineAsync(cancel);
            if (!string.IsNullOrEmpty(line)) stringBuilder.AppendLine(line);
        } while (!string.IsNullOrEmpty(line));
        return stringBuilder.Length == 0 ? null : stringBuilder.ToString();
    }

    public ValueTask WriteAssistantMessageAsync(string role, string message, CancellationToken cancel)
    {
        Console.WriteLine($"{role} > {message}");
        return ValueTask.CompletedTask;
    }
}
