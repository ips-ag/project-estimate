using Microsoft.AspNetCore.Mvc;
using ProjectEstimate.Application.Models;
using ProjectEstimate.Repositories.Agents.Consultant;

namespace ProjectEstimate.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConversationController : ControllerBase
{
    private readonly ConsultantAgent _agent;

    public ConversationController(IServiceProvider services)
    {
        _agent = services.GetRequiredService<ConsultantAgent>();
    }

    [HttpPost("", Name = nameof(CompleteConversation))]
    [ProducesResponseType(typeof(ChatCompletionResponseModel), 200)]
    public async Task<ChatCompletionResponseModel> CompleteConversation(
        [FromBody] ChatCompletionRequestModel requestModel,
        CancellationToken cancel)
    {
        string? response = await _agent.ExecuteAsync(requestModel.Input, cancel);
        return new ChatCompletionResponseModel { Output = response };
    }
}
