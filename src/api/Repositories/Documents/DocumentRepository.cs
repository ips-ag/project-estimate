using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using ProjectEstimate.Domain;
using ProjectEstimate.Repositories.Configuration;

namespace ProjectEstimate.Repositories.Documents;

internal class DocumentRepository : IDocumentRepository
{
    private readonly AzureStorageAccountSettings _settings;

    public DocumentRepository(IOptions<AzureStorageAccountSettings> options)
    {
        _settings = options.Value;
    }

    public async ValueTask<string?> CreateDocumentAsync(UserFile file, CancellationToken cancel)
    {
        var content = file.Content;
        string extension = file.Type.GetFileExtension();
        BlobContainerClient container = new(_settings.ConnectionString, "public");
        await container.CreateIfNotExistsAsync(cancellationToken: cancel);
        var blob = container.GetBlobClient($"{Guid.NewGuid():N}{extension}");
        var response = await blob.UploadAsync(content, cancellationToken: cancel);
        if (!response.HasValue) return null;
        var location = blob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddDays(7));
        return location.OriginalString;
    }

    public async ValueTask<string?> ReadDocumentAsync(string? location, CancellationToken cancel)
    {
        if (location is null) return null;
        BlobClient blob = new(new Uri(location), new BlobClientOptions());
        var result = await blob.DownloadContentAsync(cancel);
        using var reader = new StreamReader(result.Value.Content.ToStream());
        string content = await reader.ReadToEndAsync(cancel);
        return content;
    }
}
