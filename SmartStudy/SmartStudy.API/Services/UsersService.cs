
using System.Text.Json;
using SmartStudy.Models;

namespace SmartStudy.API.Services;

public class UsersService
{
    private readonly IWebHostEnvironment _env;
    private readonly string _file;

    public UsersService(IWebHostEnvironment env)
    {
        _env = env;
        _file = Path.Combine(_env.ContentRootPath, "SmartStudy.API", "Data", "Users.json");
    }

    public async Task<UserDTO?> ValidateLoginAsync(string username, string password)
    {
        if (!File.Exists(_file)) return null;

        using var s = File.OpenRead(_file);
        var users = await JsonSerializer.DeserializeAsync<List<UserDTO>>(s);

        return users?.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)
            && u.Password == password  // For hackathon: plain match. (Later: hash)
        );
    }

    public async Task<UserDTO?> GetUserByIdAsync(string id)
    {
        if (!File.Exists(_file)) return null;

        using var s = File.OpenRead(_file);
        var users = await JsonSerializer.DeserializeAsync<List<UserDTO>>(s, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return users?.FirstOrDefault(u => u.Id == id);
    }
}
