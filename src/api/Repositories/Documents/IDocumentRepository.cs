namespace ProjectEstimate.Repositories.Documents;

public interface IDocumentRepository
{
    public ValueTask<string?> CreateDocumentAsync(BinaryData content, string extension, CancellationToken cancel);
}
