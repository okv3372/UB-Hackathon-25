namespace SmartStudy.Models;

public class Enrollment
{
    // Class Id (references SchoolClass.Id)
    public string ClassId { get; set; } = string.Empty;

    // Student User Id (references User.Id)
    public string StudentId { get; set; } = string.Empty;
}
