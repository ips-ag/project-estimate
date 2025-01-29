using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectEstimate.Agents.Consultant;

namespace ProjectEstimate;

internal class ChatService : BackgroundService
{
    private readonly ConsultantAgent _agent;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<ChatService> _logger;

    public ChatService(ConsultantAgent agent, IHostApplicationLifetime applicationLifetime, ILogger<ChatService> logger)
    {
        _agent = agent;
        _applicationLifetime = applicationLifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _agent.ExecuteAsync(stoppingToken);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unhandled exception occurred");
        }
        finally
        {
            _applicationLifetime.StopApplication();
        }
    }
}
