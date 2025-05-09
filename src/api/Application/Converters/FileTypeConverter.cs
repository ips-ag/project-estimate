using System.Net.Mime;
using ProjectEstimate.Domain;

namespace ProjectEstimate.Application.Converters;

public class FileTypeConverter
{
    public FileType? ToDomain(string? contentType)
    {
        return contentType?.ToLowerInvariant() switch
        {
            MediaTypeNames.Text.Plain => FileType.Text,
            MediaTypeNames.Application.Pdf => FileType.Pdf,
            MediaTypeNames.Text.Html => FileType.Html,
            MediaTypeNames.Image.Jpeg => FileType.Jpeg,
            MediaTypeNames.Image.Png => FileType.Png,
            MediaTypeNames.Image.Bmp => FileType.Bmp,
            MediaTypeNames.Image.Tiff => FileType.Tiff,
            "image/heif" => FileType.Heif,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => FileType.Docx,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => FileType.Xlsx,
            "application/vnd.openxmlformats-officedocument.presentationml.presentation" => FileType.Pptx,
            _ => null
        };
    }
}
