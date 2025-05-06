namespace ProjectEstimate.Repositories.Documents;

public interface IDocumentRepository
{
    public ValueTask<string> CreateDocumentAsync(Stream content, CancellationToken cancel);
}
