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
        _usersPath   = Path.Combine(dataRoot, "Users.json");
    }

    // 1) Existing method: make it null-safe and concise
    public async Task<IReadOnlyList<SchoolClassDTO>> GetClassesForUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return Array.Empty<SchoolClassDTO>();

        var users = await LoadUsersAsync();
        var user  = users.FirstOrDefault(u => u.Id.Equals(userId, StringComparison.OrdinalIgnoreCase));
        var userClassIds = user?.Classes ?? new List<string>();   // <-- null-safe

        if (userClassIds.Count == 0) return Array.Empty<SchoolClassDTO>();

        var classes = await LoadClassesAsync();
        if (classes.Count == 0) return Array.Empty<SchoolClassDTO>();

        var classesById = classes.ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);

        var result = new List<SchoolClassDTO>(userClassIds.Count);
        foreach (var classId in userClassIds)
        {
            if (classesById.TryGetValue(classId, out var c))
            {
                result.Add(new SchoolClassDTO
                {
                    Id         = c.Id,
                    Name       = c.Name,
                    TeacherId  = c.TeacherId,
                    StudentIds = c.StudentIds?.ToList() ?? new List<string>()
                });
            }
        }
        return result;
    }

    // 2) NEW: get a single class by id (used by Class page)
    public async Task<SchoolClassDTO?> GetClassByIdAsync(string classId)
    {
        if (string.IsNullOrWhiteSpace(classId)) return null;
        var classes = await LoadClassesAsync();
        return classes.FirstOrDefault(c => c.Id.Equals(classId, StringComparison.OrdinalIgnoreCase));
    }

    // 3) NEW: resolve class.StudentIds -> actual student users
    public async Task<List<UserDTO>> GetStudentsForClassAsync(string classId)
    {
        var schoolClass = await GetClassByIdAsync(classId);
        if (schoolClass is null || schoolClass.StudentIds is null || schoolClass.StudentIds.Count == 0)
            return new List<UserDTO>();

        var targetIds = new HashSet<string>(schoolClass.StudentIds, StringComparer.OrdinalIgnoreCase);
        var users     = await LoadUsersAsync();

        return users
            .Where(u => targetIds.Contains(u.Id))
            .Where(u => string.Equals(u.Role, "Student", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // ---- private loaders (unchanged) ----
    private async Task<List<SchoolClassDTO>> LoadClassesAsync()
    {
        if (!File.Exists(_classesPath)) return new();
        await using var stream = File.OpenRead(_classesPath);
        return await JsonSerializer.DeserializeAsync<List<SchoolClassDTO>>(stream, JsonOptions) ?? new();
    }

    private async Task<List<UserDTO>> LoadUsersAsync()
    {
        if (!File.Exists(_usersPath)) return new();
        await using var stream = File.OpenRead(_usersPath);
        return await JsonSerializer.DeserializeAsync<List<UserDTO>>(stream, JsonOptions) ?? new();
    }
}
