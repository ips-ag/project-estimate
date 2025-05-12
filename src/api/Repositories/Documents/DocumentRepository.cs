using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using ProjectEstimate.Domain;
using ProjectEstimate.Repositories.Configuration;
using ProjectEstimate.Repositories.Documents.Converters;

namespace ProjectEstimate.Repositories.Documents;

internal class DocumentRepository : IDocumentRepository
{
    private readonly AzureStorageAccountSettings _settings;
    private readonly ContentExtractor _contentExtractor;
    private readonly FileTypeConverter _fileTypeConverter;

    public DocumentRepository(
        IOptions<AzureStorageAccountSettings> options,
        ContentExtractor contentExtractor,
        FileTypeConverter fileTypeConverter)
    {
        _contentExtractor = contentExtractor;
        _fileTypeConverter = fileTypeConverter;
        _settings = options.Value;
    }

    public async ValueTask<string?> CreateDocumentAsync(UserFile file, CancellationToken cancel)
    {
        var content = file.Content;
        string extension = file.Type.GetFileExtension();
        BlobContainerClient container = new(_settings.ConnectionString, "public");
        await container.CreateIfNotExistsAsync(cancellationToken: cancel);
        var blob = container.GetBlobClient($"{Guid.NewGuid():N}{extension}");
        string contentType = _fileTypeConverter.ToModel(file.Type);
        var headers = new BlobHttpHeaders { ContentType = contentType };
        var response = await blob.UploadAsync(
            content,
            new BlobUploadOptions { HttpHeaders = headers },
            cancellationToken: cancel);
        if (!response.HasValue) return null;
        var location = blob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddDays(7));
        return location.OriginalString;
    }

    public async ValueTask<string?> ReadDocumentAsync(string? location, CancellationToken cancel)
    {
        if (location is null) return null;
        return await _contentExtractor.ExtractTextAsync(location, cancel);
    }
}
