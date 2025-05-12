using ProjectEstimate.Domain;

namespace ProjectEstimate.Repositories.Documents.Converters;

internal class FileTypeConverter
{
    public string ToModel(FileType type)
    {
        return type switch
        {
            FileType.Text => "text/plain",
            FileType.Html => "text/html",
            FileType.Markdown => "text/markdown",
            FileType.Pdf => "application/pdf",
            FileType.Jpeg => "image/jpeg",
            FileType.Png => "image/png",
            FileType.Bmp => "image/bmp",
            FileType.Tiff => "image/tiff",
            FileType.Heif => "image/heif",
            FileType.Docx => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileType.Pptx => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported file type")
        };
    }

    public FileType? ToDomain(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "text/plain" => FileType.Text,
            "text/html" => FileType.Html,
            "text/markdown" => FileType.Markdown,
            "application/pdf" => FileType.Pdf,
            "image/jpeg" => FileType.Jpeg,
            "image/png" => FileType.Png,
            "image/bmp" => FileType.Bmp,
            "image/tiff" => FileType.Tiff,
            "image/heif" => FileType.Heif,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => FileType.Docx,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => FileType.Xlsx,
            "application/vnd.openxmlformats-officedocument.presentationml.presentation" => FileType.Pptx,
            _ => null
        };
    }
}
