namespace SmartStudy.Models;

public class ProfileDTO
{
    // School-specific student identifier
    public string StudentId { get; set; } = string.Empty;

    // URL to profile picture
    public string PictureUrl { get; set; } = string.Empty;

    // Short biography or notes about the student
    public string Bio { get; set; } = string.Empty;

    // Grade level (e.g., "9", "10", "11", "12")
    public string GradeLevel { get; set; } = string.Empty;

    // Guardian information
    public string GuardianName { get; set; } = string.Empty;
    public string GuardianEmail { get; set; } = string.Empty;
}
