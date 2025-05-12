using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using ProjectEstimate.Domain;
using ProjectEstimate.Repositories.Configuration;
using ProjectEstimate.Repositories.Documents.Converters;

namespace ProjectEstimate.Repositories.Documents;

internal class ContentExtractor
{
    private readonly FileTypeConverter _fileTypeConverter;
    private readonly ILogger<ContentExtractor> _logger;
    private readonly IOptionsSnapshot<AzureDocumentIntelligenceSettings> _options;

    public ContentExtractor(
        FileTypeConverter fileTypeConverter,
        ILogger<ContentExtractor> logger,
        IOptionsSnapshot<AzureDocumentIntelligenceSettings> options)
    {
        _fileTypeConverter = fileTypeConverter;
        _logger = logger;
        _options = options;
    }

    public async ValueTask<string?> ExtractTextAsync(string location, CancellationToken cancel)
    {
        BlobClient blob = new(new Uri(location), new BlobClientOptions());
        var result = await blob.DownloadContentAsync(cancel);
        if (!result.HasValue) return null;
        var fileType = _fileTypeConverter.ToDomain(result.Value.Details.ContentType);
        if (fileType is null)
        {
            _logger.LogWarning("Unsupported content type: {ContentType}", result.Value.Details.ContentType);
            return null;
        }

        switch (fileType)
        {
            case FileType.Text:
            case FileType.Html:
            case FileType.Markdown:
                return result.Value.Content.ToString();
            case FileType.Pdf:
            case FileType.Jpeg:
            case FileType.Png:
            case FileType.Bmp:
            case FileType.Tiff:
            case FileType.Heif:
            case FileType.Docx:
            case FileType.Xlsx:
            case FileType.Pptx:
                return await ExtractOcrAsync(result.Value.Content, cancel);
            default:
                _logger.LogWarning("Unsupported file type: {FileType}", fileType);
                return null;
        }
    }

    private async ValueTask<string?> ExtractOcrAsync(BinaryData data, CancellationToken cancel)
    {
        var settings = _options.Value;
        DocumentIntelligenceClient client = new(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey));
        var result = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-read", data, cancel);
        if (!result.HasCompleted && !result.HasValue)
        {
            _logger.LogWarning("Failed to extract data from the document using Azure AI Document Intelligence");
            return null;
        }
        return result.Value.Content;
    }
}
