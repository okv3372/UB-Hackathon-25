namespace SmartStudy.Models;

public class PracticeSet
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string ClassId { get; set; } = string.Empty;
    // Source assignment id that this practice set is derived from
    public string SrcAssignmentId { get; set; } = string.Empty;
    // File path where the practice set is stored
    public string FilePath { get; set; } = string.Empty;
    // Optional notes for the practice set
    public string Notes { get; set; } = string.Empty;
}
