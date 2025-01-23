using Microsoft.Extensions.Hosting;
using ProjectEstimate.Agents.Estimator;

namespace ProjectEstimate;

internal class ChatService : BackgroundService
{
    private readonly EstimatorAgent _agent;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public ChatService(EstimatorAgent agent, IHostApplicationLifetime applicationLifetime)
    {
        _agent = agent;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                bool shouldStop = await _agent.ExecuteAsync(stoppingToken);
                if (shouldStop) break;
            }
        }
        finally
        {
            _applicationLifetime.StopApplication();
        }
    }
}
