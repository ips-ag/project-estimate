namespace ProjectEstimate.Domain;

public enum FileType
{
    Text,
    Html,
    Markdown,
    Pdf,
    Jpeg,
    Png,
    Bmp,
    Tiff,
    Heif,
    Docx,
    Xlsx,
    Pptx
}

public static class FileTypeExtensions
{
    public static string GetFileExtension(this FileType type)
    {
        return type switch
        {
            FileType.Text => ".txt",
            FileType.Html => ".html",
            FileType.Markdown => ".md",
            FileType.Pdf => ".pdf",
            FileType.Jpeg => ".jpg",
            FileType.Png => ".png",
            FileType.Bmp => ".bmp",
            FileType.Tiff => ".tiff",
            FileType.Heif => ".heif",
            FileType.Docx => ".docx",
            FileType.Xlsx => ".xlsx",
            FileType.Pptx => ".pptx",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported file type")
        };
    }
}
