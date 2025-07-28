using Microsoft.AspNetCore.Mvc;

namespace ProjectEstimate.Controllers;

[Route("")]
[ApiController]
public class KeepAliveController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IResult Get()
    {
        return Results.Ok();
    }
}
