using ProjectEstimate.Domain;

namespace ProjectEstimate.Repositories.Documents;

public interface IDocumentRepository
{
    ValueTask<string?> CreateDocumentAsync(UserFile file, CancellationToken cancel);
    ValueTask<string?> ReadDocumentAsync(string? location, CancellationToken cancel);
}
