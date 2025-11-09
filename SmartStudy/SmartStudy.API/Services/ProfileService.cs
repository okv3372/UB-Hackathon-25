using System.Text.Json;
using SmartStudy.Models;

namespace SmartStudy.Services;

// Minimal service for hackathon demo: reads from JSON file and returns ProfileDTO by userId
public static class ProfileService
{
    public static ProfileDTO? GetProfile(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return null;

        // Simple relative path; adjust as needed if running from different working directory
        var path = Path.Combine(Directory.GetCurrentDirectory(), "SmartStudy.API", "Data", "Profiles.Json");
        if (!File.Exists(path)) return null;

        try
        {
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var records = JsonSerializer.Deserialize<List<ProfileRecord>>(json, options) ?? new List<ProfileRecord>();
            var match = records.FirstOrDefault(r => string.Equals(r.UserId, userId, StringComparison.OrdinalIgnoreCase));
            if (match == null) return null;

            return new ProfileDTO
            {
                StudentId = match.StudentId,
                PictureUrl = match.PictureUrl,
                Name = match.Name,
                Bio = match.Bio,
                GradeLevel = match.GradeLevel,
                GuardianName = match.GuardianName,
                GuardianEmail = match.GuardianEmail
            };
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Update a profile using individual values corresponding to ProfileDTO properties.
    /// Uses StudentId as the record key. Creates a new entry if not found.
    /// Returns the updated ProfileDTO.
    /// </summary>
    public static ProfileDTO UpdateProfileFromValues(
        string? studentId,
        string? pictureUrl,
        string? name,
        string? bio,
        string? gradeLevel,
        string? guardianName,
        string? guardianEmail)
    {
        var sid = studentId ?? string.Empty;
        var pic = pictureUrl ?? string.Empty;
        var nm = name ?? string.Empty;
        var b = bio ?? string.Empty;
        var gl = gradeLevel ?? string.Empty;
        var gName = guardianName ?? string.Empty;
        var gEmail = guardianEmail ?? string.Empty;

        // Minimal, forgiving implementation (similar to UpdateProfile)
        var path = Path.Combine(Directory.GetCurrentDirectory(), "SmartStudy.API", "Data", "Profiles.Json");

        List<ProfileRecord> records;
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                records = JsonSerializer.Deserialize<List<ProfileRecord>>(json, options) ?? new List<ProfileRecord>();
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                records = new List<ProfileRecord>();
            }
        }
        catch
        {
            records = new List<ProfileRecord>();
        }

        // Use StudentId as key for this helper
        var existing = records.FirstOrDefault(r => string.Equals(r.StudentId, sid, StringComparison.OrdinalIgnoreCase));
        if (existing == null)
        {
            existing = new ProfileRecord { StudentId = sid };
            records.Add(existing);
        }

        // Update fields from supplied values (empty string counts as update)
        existing.PictureUrl = pic;
        existing.Name = nm;
        existing.Bio = b;
        existing.GradeLevel = gl;
        existing.GuardianName = gName;
        existing.GuardianEmail = gEmail;

        try
        {
            var writeOptions = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson = JsonSerializer.Serialize(records, writeOptions);
            File.WriteAllText(path, updatedJson);
        }
        catch
        {
            // Ignore write errors for hackathon demo
        }

        return new ProfileDTO
        {
            StudentId = existing.StudentId,
            PictureUrl = existing.PictureUrl,
            Name = existing.Name,
            Bio = existing.Bio,
            GradeLevel = existing.GradeLevel,
            GuardianName = existing.GuardianName,
            GuardianEmail = existing.GuardianEmail
        };
    }

    public static ProfileDTO UpdateProfile(string userId, string studentId, string? pictureUrl, string? bio, string? gradeLevel, string? guardianName, string? guardianEmail)
    {
        // Minimal, forgiving implementation: update existing or create if missing, then return DTO
        var path = Path.Combine(Directory.GetCurrentDirectory(), "SmartStudy.API", "Data", "Profiles.Json");

        List<ProfileRecord> records;
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                records = JsonSerializer.Deserialize<List<ProfileRecord>>(json, options) ?? new List<ProfileRecord>();
            }
            else
            {
                // Ensure directory exists before writing later
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                records = new List<ProfileRecord>();
            }
        }
        catch
        {
            // On any read/parse error, start fresh to keep demo moving
            records = new List<ProfileRecord>();
        }

        var existing = records.FirstOrDefault(r => string.Equals(r.UserId, userId, StringComparison.OrdinalIgnoreCase));
        if (existing == null)
        {
            existing = new ProfileRecord { UserId = userId };
            records.Add(existing);
        }

        // Always update required StudentId; others only if provided (non-null)
        existing.StudentId = studentId;
        if (pictureUrl != null) existing.PictureUrl = pictureUrl;
        if (bio != null) existing.Bio = bio;
        if (gradeLevel != null) existing.GradeLevel = gradeLevel;
        if (guardianName != null) existing.GuardianName = guardianName;
        if (guardianEmail != null) existing.GuardianEmail = guardianEmail;

        try
        {
            var writeOptions = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson = JsonSerializer.Serialize(records, writeOptions);
            File.WriteAllText(path, updatedJson);
        }
        catch
        {
            // Ignore write errors for hackathon demo; still return the DTO representation
        }

        return new ProfileDTO
        {
            StudentId = existing.StudentId,
            PictureUrl = existing.PictureUrl,
            Name = existing.Name,
            Bio = existing.Bio,
            GradeLevel = existing.GradeLevel,
            GuardianName = existing.GuardianName,
            GuardianEmail = existing.GuardianEmail
        };
    }

    private class ProfileRecord
    {
        public string UserId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string PictureUrl { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;
        public string GuardianName { get; set; } = string.Empty;
        public string GuardianEmail { get; set; } = string.Empty;
    }
}

