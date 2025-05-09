using System.Text.Json.Serialization;

namespace ProjectEstimate.Application.Models;

public class FileUploadResponseModel
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}
