namespace SmartStudy.Models;

public class AssignmentDTO
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string ClassId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    // Original file name uploaded by student
    public string FileName { get; set; } = string.Empty;

    // Path where the file is stored (relative or absolute depending on app config)
    public string FilePath { get; set; } = string.Empty;

    // Comments left by the teacher
    public string TeacherComments { get; set; } = string.Empty;

    // Extracted fulltext content of the uploaded PDF (if available)
    public string ExtractedText { get; set; } = string.Empty;
    public string PracticeSetId { get; set; } = string.Empty;
}
