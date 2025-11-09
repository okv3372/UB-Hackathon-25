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
    /// Never throws; returns empty string on failure and logs diagnostics to Console.
    /// </summary>
    public static string ExtractText(string filePath)
    {
        try
        {
            Console.WriteLine($"[PDFPig] Starting extraction. Path='{filePath}'");
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("[PDFPig] FAILED: filePath was null/empty.");
                return string.Empty;
            }
            if (!File.Exists(filePath))
            {
                Console.WriteLine("[PDFPig] FAILED: File does not exist.");
                return string.Empty;
            }
            var info = new FileInfo(filePath);
            Console.WriteLine($"[PDFPig] File exists. Size={info.Length} bytes");

            var sb = new System.Text.StringBuilder();
            using var document = PdfDocument.Open(filePath);
            Console.WriteLine($"[PDFPig] Pages={document.NumberOfPages}");
            for (int i = 1; i <= document.NumberOfPages; i++)
            {
                string pageText = string.Empty;
                try
                {
                    var page = document.GetPage(i);
                    pageText = page?.Text ?? string.Empty;
                }
                catch (Exception exPage)
                {
                    Console.WriteLine($"[PDFPig] WARN: Failed to read page {i}: {exPage.Message}");
                }
                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    if (sb.Length > 0) sb.AppendLine();
                    sb.AppendLine(pageText.Trim());
                }
            }
            Console.WriteLine("[PDFPig] Extraction complete.");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PDFPig] ERROR: Extraction failed: {ex.Message}");
            return string.Empty;
        }
    }
}
