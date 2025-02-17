using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ProjectEstimate.Application.Models;
using ProjectEstimate.Repositories.Agents;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace ProjectEstimate;

public class Function
{
    private readonly ILogger<Function> _logger;
    private readonly TestAgent _agent;

    public Function(ILogger<Function> logger, TestAgent agent)
    {
        _logger = logger;
        _agent = agent;
    }

    [Function("CompleteConversation")]
    public async Task<IActionResult> Complete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "conversation")]
        HttpRequest req,
        [FromBody] ChatCompletionRequestModel requestModel,
        CancellationToken cancel)
    {
        string? response = await _agent.CompleteAsync(requestModel.Input, cancel);
        return new OkObjectResult(new ChatCompletionResponseModel { Output = response });
    }
}
