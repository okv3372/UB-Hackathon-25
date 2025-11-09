namespace SmartStudy.Models;

public class PracticeSetDTO
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string ClassId { get; set; } = string.Empty;
    // Source assignment id that this practice set is derived from
    public string SrcAssignmentId { get; set; } = string.Empty;
    public string Questions { get; set; } = string.Empty;
    // Optional notes for the practice set
    public string Notes { get; set; } = string.Empty;
}
