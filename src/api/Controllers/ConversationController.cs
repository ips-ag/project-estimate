using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using ProjectEstimate.Application.Models;

namespace ProjectEstimate.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConversationController : ControllerBase
{
    private readonly Channel<ChatCompletionRequestModel> _workQueue;

    public ConversationController(Channel<ChatCompletionRequestModel> workQueue)
    {
        _workQueue = workQueue;
    }

    [HttpPost("", Name = nameof(CompleteConversation))]
    [ProducesResponseType(200)]
    [ProducesResponseType(503)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public IActionResult CompleteConversation([FromBody] ChatCompletionRequestModel requestModel)
    {
        if (!_workQueue.Writer.TryWrite(requestModel))
        {
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        return Ok();
    }
}
