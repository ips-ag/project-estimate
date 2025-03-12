using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using ProjectEstimate.Application.Models;
using ProjectEstimate.Repositories.Agents.Consultant;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace ProjectEstimate;

internal class Function
{
    private readonly ConsultantAgent _agent;

    public Function(ConsultantAgent agent)
    {
        _agent = agent;
    }

    [Function("UploadFile")]
    public Task<IActionResult> UploadFile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "upload")]
        HttpRequest req,
        CancellationToken cancel)
    {
        throw new NotImplementedException();
    }

    [Function("CompleteConversation")]
    public async Task<IActionResult> CompleteConversation(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "conversation")]
        HttpRequest req,
        [FromBody] ChatCompletionRequestModel requestModel,
        CancellationToken cancel)
    {
        string? response = await _agent.ExecuteAsync(requestModel.Input, cancel);
        return new OkObjectResult(new ChatCompletionResponseModel { Output = response });
    }
}
