using System;
using System.Text;
using System.IO;
using UglyToad.PdfPig;

namespace SmartStudy.API.Services;

/// <summary>
/// Minimal PDF text extraction using the open-source PDFPig library.
/// Usage:
///   var text = PDFPigService.ExtractText("/path/to/file.pdf");
/// </summary>
public static class PDFPigService
{
    /// <summary>
    /// Extracts and returns the text content from the provided PDF file path.
    /// </summary>
    public static string ExtractText(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required.", nameof(filePath));
        if (!File.Exists(filePath))
            throw new FileNotFoundException("PDF file not found.", filePath);
        var sb = new System.Text.StringBuilder();
        using (var document = PdfDocument.Open(filePath))
        {
            for (int i = 1; i <= document.NumberOfPages; i++)
            {
                var page = document.GetPage(i);
                var text = page?.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    if (sb.Length > 0) sb.AppendLine();
                    sb.AppendLine(text.Trim());
                }
            }
        }
        return sb.ToString();
    }
}
