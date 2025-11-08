namespace SmartStudy.Models;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // In production store hashed passwords
    public string Role { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<string> Classes { get; set; } = new();
}
