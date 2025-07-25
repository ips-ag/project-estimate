using System.Threading.Channels;
using ProjectEstimate.Application.Models;
using ProjectEstimate.Application.Request.Context;
using ProjectEstimate.Domain;
using ProjectEstimate.Repositories.Agents.Consultant;

namespace ProjectEstimate.Application.Agents;

public class AgentBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AgentBackgroundService> _logger;
    private readonly Channel<ChatCompletionRequestModel> _workQueue;

    public AgentBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<AgentBackgroundService> logger,
        Channel<ChatCompletionRequestModel> workQueue)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _workQueue = workQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(AgentBackgroundService)} starting");
        await foreach (var requestModel in _workQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogDebug("Processing agent work item");
                await using var scope = _serviceProvider.CreateAsyncScope();
                var contextAccessor = scope.ServiceProvider.GetRequiredService<IRequestContextAccessor>();
                contextAccessor.Context = new RequestContext(requestModel.ConnectionId);
                var agent = scope.ServiceProvider.GetRequiredService<ConsultantAgent>();
                var request = new ChatCompletionRequest(requestModel.Input, requestModel.FileInput);
                await agent.ExecuteAsync(request, stoppingToken);
                _logger.LogDebug("Agent work item completed successfully");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("Agent work item was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing agent work item");
            }
        }
    }
}
