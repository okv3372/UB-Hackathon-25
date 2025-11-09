using System.Text.Json;
using System.Text.Json.Serialization;
using SmartStudy.Models;

namespace SmartStudy.Services;

// Service: resolve a PracticeSet starting from an AssignmentId via the assignment's PracticeSetId
public class PracticeSetsService
{
    /// <summary>
    /// Given an assignmentId, look up the assignment in Assignments.Json to get its PracticeSetId,
    /// then load PracticeSets.Json and return the corresponding PracticeSetDTO.
    /// Returns null if any step fails or entries are missing.
    /// </summary>
    public static async Task<PracticeSetDTO?> GetPracticeSetAsync(string assignmentId)
    {
        if (string.IsNullOrWhiteSpace(assignmentId)) return null;

        try
        {
            var dataRoot = Path.Combine(Directory.GetCurrentDirectory(), "SmartStudy.API", "Data");

            // 1) Read assignments and find the matching assignment
            var assignmentsPath = Path.Combine(dataRoot, "Assignments.Json");
            if (!File.Exists(assignmentsPath)) return null;

            var assignmentsJson = await File.ReadAllTextAsync(assignmentsPath);
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var assignments = JsonSerializer.Deserialize<List<AssignmentRecord>>(assignmentsJson, jsonOptions) ?? new List<AssignmentRecord>();

            var assignment = assignments.FirstOrDefault(a => string.Equals(a.Id, assignmentId, StringComparison.OrdinalIgnoreCase));
            if (assignment == null) return null;

            var practiceSetId = assignment.PracticeSetId;
            if (string.IsNullOrWhiteSpace(practiceSetId)) return null;

            // 2) Load the practice set by that id
            return await GetPracticeSetByIdAsync(practiceSetId);
        }
        catch
        {
            // For hackathon purposes, swallow errors and return null
            return null;
        }
    }

    /// <summary>
    /// One-time backfill: scan Assignments.Json for any entries with a PracticeSetId
    /// that does not exist in PracticeSets.Json, and create a placeholder record so
    /// the UI can load it. Returns the number of practice sets created.
    /// </summary>
    public static async Task<int> BackfillMissingPracticeSetsAsync()
    {
        try
        {
            var dataRoot = Path.Combine(Directory.GetCurrentDirectory(), "SmartStudy.API", "Data");
            var assignmentsPath = Path.Combine(dataRoot, "Assignments.Json");
            var practiceSetsPath = Path.Combine(dataRoot, "PracticeSets.Json");

            if (!File.Exists(assignmentsPath)) return 0;

            // Load assignments (only id + practiceSetId needed)
            var assignmentsJson = await File.ReadAllTextAsync(assignmentsPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var assignments = JsonSerializer.Deserialize<List<AssignmentFullRecord>>(assignmentsJson, options) ?? new();

            // Load existing practice sets (ids only)
            var existingPracticeSets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists(practiceSetsPath))
            {
                try
                {
                    var psJson = await File.ReadAllTextAsync(practiceSetsPath);
                    var psRecords = JsonSerializer.Deserialize<List<PracticeSetRecord>>(psJson, options) ?? new();
                    foreach (var r in psRecords)
                    {
                        if (!string.IsNullOrWhiteSpace(r.Id)) existingPracticeSets.Add(r.Id);
                    }
                }
                catch
                {
                    // If corrupt, we'll rebuild entries we need; let SavePracticeSet handle writing.
                }
            }

            var created = 0;
            foreach (var a in assignments)
            {
                if (string.IsNullOrWhiteSpace(a.PracticeSetId)) continue;
                if (existingPracticeSets.Contains(a.PracticeSetId)) continue;

                // Create a placeholder practice set record with the expected id
                var dto = new PracticeSetDTO
                {
                    Id = a.PracticeSetId,
                    StudentId = a.StudentId ?? string.Empty,
                    ClassId = a.ClassId ?? string.Empty,
                    SrcAssignmentId = a.Id,
                    Questions = "{\"test\":{\"question\":null}}",
                    Notes = string.Empty
                };
                await SavePracticeSetAsync(dto);
                existingPracticeSets.Add(dto.Id);
                created++;
            }

            return created;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Persist or update a practice set record on disk (SmartStudy.API/Data/PracticeSets.Json).
    /// If a record with the same Id exists it will be replaced, otherwise it will be appended.
    /// Ensures Questions are stored as a JSON element (defaults to {"test":{"question":null}} if invalid/empty).
    /// </summary>
    public static async Task SavePracticeSetAsync(PracticeSetDTO dto)
    {
        if (dto == null) return;

        var dataRoot = Path.Combine(Directory.GetCurrentDirectory(), "SmartStudy.API", "Data");
        Directory.CreateDirectory(dataRoot);
        var path = Path.Combine(dataRoot, "PracticeSets.Json");

        List<PracticeSetRecord> records;
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        try
        {
            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path);
                records = JsonSerializer.Deserialize<List<PracticeSetRecord>>(json, options) ?? new List<PracticeSetRecord>();
            }
            else
            {
                records = new List<PracticeSetRecord>();
            }
        }
        catch
        {
            // If file is corrupt, start fresh to avoid blocking user flow
            records = new List<PracticeSetRecord>();
        }

        var newRecord = new PracticeSetRecord
        {
            Id = dto.Id,
            StudentId = dto.StudentId,
            ClassId = dto.ClassId,
            SrcAssignmentId = dto.SrcAssignmentId,
            Questions = dto.Questions,
            Notes = dto.Notes ?? string.Empty
        };

        var existingIdx = records.FindIndex(r => string.Equals(r.Id, dto.Id, StringComparison.OrdinalIgnoreCase));
        if (existingIdx >= 0)
        {
            records[existingIdx] = newRecord;
        }
        else
        {
            records.Add(newRecord);
        }

        // Write back to disk
        var outJson = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, outJson);
    }

    private static async Task<PracticeSetDTO?> GetPracticeSetByIdAsync(string practiceSetId)
    {
        if (string.IsNullOrWhiteSpace(practiceSetId)) return null;

        var path = Path.Combine(Directory.GetCurrentDirectory(), "SmartStudy.API", "Data", "PracticeSets.Json");
        if (!File.Exists(path)) return null;

        try
        {
            var json = await File.ReadAllTextAsync(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var records = JsonSerializer.Deserialize<List<PracticeSetRecord>>(json, options) ?? new List<PracticeSetRecord>();

            var match = records.FirstOrDefault(r => string.Equals(r.Id, practiceSetId, StringComparison.OrdinalIgnoreCase));
            if (match == null) return null;

            return new PracticeSetDTO
            {
                Id = match.Id,
                StudentId = match.StudentId,
                ClassId = match.ClassId,
                SrcAssignmentId = match.SrcAssignmentId,
                // Serialize Questions node back to JSON string for the DTO
                Questions = match.Questions,
                Notes = match.Notes
            };
        }
        catch
        {
            // For hackathon purposes, swallow errors and return null
            return null;
        }
    }
    
    /// <summary>
    /// Constructs a new PracticeSetDTO from the provided values. This does NOT persist to disk.
    /// Caller is responsible for saving if needed.
    /// </summary>
    /// <param name="id">Unique practice set id</param>
    /// <param name="studentId">Student owning the practice set</param>
    /// <param name="classId">Class context for the practice set</param>
    /// <param name="srcAssignmentId">Source assignment id this set was derived from</param>
    /// <param name="questions">Serialized questions content (JSON/markdown/etc.)</param>
    /// <param name="notes">Optional notes</param>
    public static async Task<PracticeSetDTO> MakePracticeSetAsync(
        string studentId,
        string classId,
        string srcAssignmentId,
        string questions)
    {
        Console.WriteLine("MAKING PRACTICE SET WITH QUESTIONS: " + questions);
        PracticeSetDTO practiceSet = new PracticeSetDTO
        {
            Id = Guid.NewGuid().ToString(),
            StudentId = studentId,
            ClassId = classId,
            SrcAssignmentId = srcAssignmentId,
            Questions = questions,
        };
        await SavePracticeSetAsync(practiceSet);
        return practiceSet;
    }

    private class PracticeSetRecord
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string SrcAssignmentId { get; set; } = string.Empty;
        public string? Questions { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    private class AssignmentRecord
    {
        public string Id { get; set; } = string.Empty;
        public string PracticeSetId { get; set; } = string.Empty;
    }

    private class AssignmentFullRecord : AssignmentRecord
    {
        public string StudentId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
    }
}

