using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ProjectEstimate.Application.Models;

namespace ProjectEstimate.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConversationController : ControllerBase
{
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly Channel<ChatCompletionRequestModel> _workQueue;

    public ConversationController(
        ProblemDetailsFactory problemDetailsFactory,
        Channel<ChatCompletionRequestModel> workQueue)
    {
        _problemDetailsFactory = problemDetailsFactory;
        _workQueue = workQueue;
    }

    [HttpPost("", Name = nameof(CompleteConversation))]
    [ProducesResponseType(typeof(ChatCompletionResponseModel), 200)]
    public IActionResult CompleteConversation([FromBody] ChatCompletionRequestModel requestModel)
    {
        if (!_workQueue.Writer.TryWrite(requestModel))
        {
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        var responseModel = new ChatCompletionResponseModel { Output = string.Empty };
        return Ok(responseModel);
    }
}
