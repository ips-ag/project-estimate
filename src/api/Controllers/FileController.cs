using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectEstimate.Application.Converters;
using ProjectEstimate.Application.Models;
using ProjectEstimate.Domain;
using ProjectEstimate.Repositories.Agents.Consultant;

namespace ProjectEstimate.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FileController : ControllerBase
{
    private readonly ConsultantAgent _agent;
    private readonly FileTypeConverter _fileTypeConverter;

    public FileController(IServiceProvider services, FileTypeConverter fileTypeConverter)
    {
        _agent = services.GetRequiredService<ConsultantAgent>();
        _fileTypeConverter = fileTypeConverter;
    }

    [HttpPost("", Name = nameof(UploadFile))]
    [ProducesResponseType(typeof(FileUploadResponseModel), 200)]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<FileUploadResponseModel> UploadFile(
        [FromForm(Name = "file")] IFormFile form,
        CancellationToken cancel)
    {
        await using var stream = form.OpenReadStream();
        var data = await BinaryData.FromStreamAsync(stream, cancel);
        var fileType = _fileTypeConverter.ToDomain(form.FileName);
        if (fileType is null)
        {
            return new FileUploadResponseModel { ErrorMessage = "Unsupported file type" };
        }
        UserFile file = new(data, fileType.Value);
        string? location = await _agent.UploadFileAsync(file, cancel);
        return string.IsNullOrEmpty(location)
            ? new FileUploadResponseModel { ErrorMessage = "Failed to upload file" }
            : new FileUploadResponseModel { Location = location };
    }
}
