namespace SmartStudy.Models;

public class SchoolClass
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    // Teacher user id (references a User with Role = "Instructor")
    public string TeacherId { get; set; } = string.Empty;
}
