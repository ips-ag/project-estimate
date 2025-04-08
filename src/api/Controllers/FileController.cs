using Microsoft.AspNetCore.Mvc;

namespace ProjectEstimate.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileController : ControllerBase
{
    [HttpPost("upload", Name = nameof(UploadFile))]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> UploadFile(CancellationToken cancel)
    {
        var result = await HttpContext.Request.BodyReader.ReadAsync(cancel);
        while (!result.IsCompleted)
        {
            // Do something with buffer
            //result.Buffer;
        }
        throw new NotImplementedException();
    }
}
