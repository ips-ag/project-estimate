using Microsoft.Extensions.Options;
using ProjectEstimate.Repositories.Configuration;

namespace ProjectEstimate.Repositories.Documents;

internal class DocumentRepository : IDocumentRepository
{
    private readonly AzureStorageAccountSettings _settings;

    public DocumentRepository(IOptions<AzureStorageAccountSettings> options)
    {
        _settings = options.Value;
        
    }
    
    public ValueTask<string> CreateDocumentAsync(Stream content, CancellationToken cancel)
    {
        throw new NotImplementedException();
    }
}
