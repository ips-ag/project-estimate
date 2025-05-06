using Microsoft.AspNetCore.Mvc;
using ProjectEstimate.Application.Models;
using ProjectEstimate.Repositories.Agents.Consultant;

namespace ProjectEstimate.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileController : ControllerBase
{
    private readonly ConsultantAgent _agent;

    public FileController(IServiceProvider services)
    {
        _agent = services.GetRequiredService<ConsultantAgent>();
    }

    [HttpPost("", Name = nameof(UploadFile))]
    [ProducesResponseType(typeof(FileUploadResponseModel), 200)]
    public async Task<FileUploadResponseModel> UploadFile(CancellationToken cancel)
    {
        var result = await HttpContext.Request.BodyReader.ReadAsync(cancel);
        await using var memoryStream = new MemoryStream();
        while (!result.IsCompleted)
        {
            if (result.IsCanceled) return new FileUploadResponseModel();
            foreach (var memory in result.Buffer)
            {
                await memoryStream.WriteAsync(memory, cancel);
            }
        }
        memoryStream.Seek(0, SeekOrigin.Begin);
        string? location = await _agent.UploadFileAsync(memoryStream, cancel);
        return new FileUploadResponseModel { Location = location };
    }
}
