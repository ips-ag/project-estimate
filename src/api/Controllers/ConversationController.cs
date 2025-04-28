using Microsoft.AspNetCore.Mvc;
using ProjectEstimate.Application.Models;
using ProjectEstimate.Application.Request.Context;
using ProjectEstimate.Repositories.Agents.Consultant;

namespace ProjectEstimate.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConversationController : ControllerBase
{
    private readonly ConsultantAgent _agent;
    private readonly IRequestContextAccessor _contextAccessor;

    public ConversationController(IServiceProvider services, IRequestContextAccessor contextAccessor)
    {
        _agent = services.GetRequiredService<ConsultantAgent>();
        _contextAccessor = contextAccessor;
    }

    [HttpPost("", Name = nameof(CompleteConversation))]
    [ProducesResponseType(typeof(ChatCompletionResponseModel), 200)]
    public async Task<ChatCompletionResponseModel> CompleteConversation(
        [FromBody] ChatCompletionRequestModel requestModel,
        CancellationToken cancel)
    {
        _contextAccessor.Context = new RequestContext(requestModel.ConnectionId);
        string? response = await _agent.ExecuteAsync(requestModel.Input, cancel);
        return new ChatCompletionResponseModel { Output = response };
    }
}
