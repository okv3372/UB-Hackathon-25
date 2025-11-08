namespace SmartStudy.Models;

public class Assignment
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
}
