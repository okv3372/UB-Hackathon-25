using System.Text.Json;
using SmartStudy.Models;

namespace SmartStudy.API.Services;

public class ClassesService
{
    private readonly string _classesPath;
    private readonly string _usersPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ClassesService(IWebHostEnvironment env)
    {
        var dataRoot = Path.Combine(env.ContentRootPath, "SmartStudy.API", "Data");
        _classesPath = Path.Combine(dataRoot, "Classes.json");
        _usersPath = Path.Combine(dataRoot, "Users.json");
    }

    public async Task<IReadOnlyList<SchoolClassDTO>> GetClassesForUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<SchoolClassDTO>();
        }

        var users = await LoadUsersAsync();
        var user = users.FirstOrDefault(u => u.Id.Equals(userId, StringComparison.OrdinalIgnoreCase));
        if (user is null || user.Classes.Count == 0)
        {
            return Array.Empty<SchoolClassDTO>();
        }

        var classes = await LoadClassesAsync();
        if (classes.Count == 0)
        {
            return Array.Empty<SchoolClassDTO>();
        }

        var classesById = classes.ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);
        var result = new List<SchoolClassDTO>();

        foreach (var classId in user.Classes)
        {
            if (!classesById.TryGetValue(classId, out var schoolClass))
            {
                continue;
            }

            result.Add(new SchoolClassDTO
            {
                Id = schoolClass.Id,
                Name = schoolClass.Name,
                TeacherId = schoolClass.TeacherId,
                StudentIds = schoolClass.StudentIds?.ToList() ?? new List<string>()
            });
        }

        return result;
    }

    private async Task<List<SchoolClassDTO>> LoadClassesAsync()
    {
        if (!File.Exists(_classesPath))
        {
            return new List<SchoolClassDTO>();
        }

        await using var stream = File.OpenRead(_classesPath);
        return await JsonSerializer.DeserializeAsync<List<SchoolClassDTO>>(stream, JsonOptions)
               ?? new List<SchoolClassDTO>();
    }

    private async Task<List<UserDTO>> LoadUsersAsync()
    {
        if (!File.Exists(_usersPath))
        {
            return new List<UserDTO>();
        }

        await using var stream = File.OpenRead(_usersPath);
        return await JsonSerializer.DeserializeAsync<List<UserDTO>>(stream, JsonOptions)
               ?? new List<UserDTO>();
    }
}
