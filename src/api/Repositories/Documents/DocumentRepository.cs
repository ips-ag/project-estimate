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
}
