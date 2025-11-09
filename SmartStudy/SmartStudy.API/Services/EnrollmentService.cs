using System.Text.Json;
using SmartStudy.Models;

namespace SmartStudy.API.Services;

public class EnrollmentService
{
    private readonly IWebHostEnvironment _env;
    private readonly UsersService _users;
    private readonly string _file;

    public EnrollmentService(IWebHostEnvironment env, UsersService users)
    {
        _env = env;
        _users = users;
        // Match your UsersService path style
        _file = Path.Combine(_env.ContentRootPath, "SmartStudy.API", "Data", "Enrollments.json");
    }

    private async Task<List<EnrollmentDTO>> LoadAsync()
    {
        if (!File.Exists(_file)) return new List<EnrollmentDTO>();
        using var s = File.OpenRead(_file);
        var items = await JsonSerializer.DeserializeAsync<List<EnrollmentDTO>>(
            s,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        return items ?? new List<EnrollmentDTO>();
    }

    // 🍏 The one method you need
    public async Task<List<UserDTO>> GetStudentsForClassAsync(string classId)
    {
        var enrollments = await LoadAsync();

        var studentIds = enrollments
            .Where(e => e.ClassId.Equals(classId, StringComparison.OrdinalIgnoreCase))
            .Select(e => e.StudentId)
            .Distinct()
            .ToList();

        var result = new List<UserDTO>();
        foreach (var sid in studentIds)
        {
            var u = await _users.GetUserByIdAsync(sid);
            if (u != null && u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase))
                result.Add(u);
        }
        return result;
    }
}
