using System.Net.Mime;
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
        var data = await BinaryData.FromStreamAsync(HttpContext.Request.Body, cancel);
        string extension = HttpContext.Request.Headers.ContentType.FirstOrDefault()?.ToLowerInvariant() switch
        {
            MediaTypeNames.Text.Plain => ".txt",
            MediaTypeNames.Application.Pdf => ".pdf",
            MediaTypeNames.Image.Jpeg => ".jpg",
            MediaTypeNames.Image.Png => ".png",
            MediaTypeNames.Image.Bmp => ".bmp",
            MediaTypeNames.Image.Tiff => ".tiff",
            "image/heif" => ".heif",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation" => ".pptx",
            _ => ".txt"
        };
        string? location = await _agent.UploadFileAsync(data, extension, cancel);
        return new FileUploadResponseModel { Location = location };
    }
}
