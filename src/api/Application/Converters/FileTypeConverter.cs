﻿using ProjectEstimate.Domain;

namespace ProjectEstimate.Application.Converters;

public class FileTypeConverter
{
    public FileType? ToDomain(string? fileName)
    {
        string? extension = Path.GetExtension(fileName);
        return extension?.ToLowerInvariant() switch
        {
            ".txt" => FileType.Text,
            ".html" => FileType.Html,
            ".md" => FileType.Markdown,
            ".markdown" => FileType.Markdown,
            ".pdf" => FileType.Pdf,
            ".jpeg" => FileType.Jpeg,
            ".jpg" => FileType.Jpeg,
            ".png" => FileType.Png,
            ".bmp" => FileType.Bmp,
            ".tiff" => FileType.Tiff,
            ".heif" => FileType.Heif,
            ".docx" => FileType.Docx,
            ".xlsx" => FileType.Xlsx,
            ".pptx" => FileType.Pptx,
            _ => null
        };
    }
}
