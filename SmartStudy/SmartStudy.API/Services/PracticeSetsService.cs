using System.Text.Json;
using SmartStudy.Models;

namespace SmartStudy.Services;

// Bare-minimum service: read from JSON and return a single PracticeSet by Id
public static class PracticeSetsService
{
    public static PracticeSetDTO? GetPracticeSet(string practiceSetId)
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
                FilePath = match.FilePath,
                Notes = match.Notes
            };
        }

        // Hackathon placeholder: create a new practice set with hard-coded values
    
		catch
		{
			// For hackathon purposes, swallow errors and return null
			return null;
		}
	}
    
    public static PracticeSetDTO MakeSet()
    {
        return new PracticeSetDTO
        {
            Id = "demo-set-1",
            StudentId = "u001",
            ClassId = "DemoClass",
            SrcAssignmentId = "a001",
            FilePath = "", // No file yet
            Notes = "Generated demo practice set (hard-coded)."
        };
    }

	private class PracticeSetRecord
	{
		public string Id { get; set; } = string.Empty;
		public string StudentId { get; set; } = string.Empty;
		public string ClassId { get; set; } = string.Empty;
		public string SrcAssignmentId { get; set; } = string.Empty;
		public string FilePath { get; set; } = string.Empty;
		public string Notes { get; set; } = string.Empty;
	}
}

