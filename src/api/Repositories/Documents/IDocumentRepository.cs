using ProjectEstimate.Domain;

namespace ProjectEstimate.Repositories.Documents;

public interface IDocumentRepository
{
    public ValueTask<string?> CreateDocumentAsync(UserFile file, CancellationToken cancel);
}
