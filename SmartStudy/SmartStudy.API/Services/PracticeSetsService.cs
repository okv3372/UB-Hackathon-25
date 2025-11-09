using System.Text.Json;
using System.Text.Json.Serialization;
using SmartStudy.Models;

namespace SmartStudy.Services;

// Service: resolve a PracticeSet starting from an AssignmentId via the assignment's PracticeSetId
public static class PracticeSetsService
{
    /// <summary>
    /// Given an assignmentId, look up the assignment in Assignments.Json to get its PracticeSetId,
    /// then load PracticeSets.Json and return the corresponding PracticeSetDTO.
    /// Returns null if any step fails or entries are missing.
    /// </summary>
    public static PracticeSetDTO? GetPracticeSet(string assignmentId)
    {
        if (string.IsNullOrWhiteSpace(assignmentId)) return null;

        try
        {
            var dataRoot = Path.Combine(Directory.GetCurrentDirectory(), "SmartStudy.API", "Data");

            // 1) Read assignments and find the matching assignment
            var assignmentsPath = Path.Combine(dataRoot, "Assignments.Json");
            if (!File.Exists(assignmentsPath)) return null;

            var assignmentsJson = File.ReadAllText(assignmentsPath);
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var assignments = JsonSerializer.Deserialize<List<AssignmentRecord>>(assignmentsJson, jsonOptions) ?? new List<AssignmentRecord>();

            var assignment = assignments.FirstOrDefault(a => string.Equals(a.Id, assignmentId, StringComparison.OrdinalIgnoreCase));
            if (assignment == null) return null;

            var practiceSetId = assignment.PracticeSetId;
            if (string.IsNullOrWhiteSpace(practiceSetId)) return null;

            // 2) Load the practice set by that id
            return GetPracticeSetById(practiceSetId);
        }
        catch
        {
            // For hackathon purposes, swallow errors and return null
            return null;
        }
    }

    private static PracticeSetDTO? GetPracticeSetById(string practiceSetId)
    {
        if (string.IsNullOrWhiteSpace(practiceSetId)) return null;

        var path = Path.Combine(Directory.GetCurrentDirectory(), "SmartStudy.API", "Data", "PracticeSets.Json");
        if (!File.Exists(path)) return null;

        try
        {
            var json = File.ReadAllText(path);
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
                Questions = match.Questions.ValueKind == JsonValueKind.Undefined
                    ? string.Empty
                    : match.Questions.GetRawText(),
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
    public static PracticeSetDTO MakePracticeSet(
        string studentId,
        string classId,
        string srcAssignmentId,
        string questions)
    {
        return new PracticeSetDTO
        {
            Id = Guid.NewGuid().ToString(),
            StudentId = studentId,
            ClassId = classId,
            SrcAssignmentId = srcAssignmentId,
            Questions = questions,
        };
    }

    private class PracticeSetRecord
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string SrcAssignmentId { get; set; } = string.Empty;
        public JsonElement Questions { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    private class AssignmentRecord
    {
        public string Id { get; set; } = string.Empty;
        public string PracticeSetId { get; set; } = string.Empty;
    }
}

