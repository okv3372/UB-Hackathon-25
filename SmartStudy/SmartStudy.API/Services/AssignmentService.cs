using System.Text.Json;
using SmartStudy.Models;
using SmartStudy.Services;
using SmartStudy.API.SemanticKernel;

namespace SmartStudy.API.Services;

public class AssignmentService
{
	private readonly string _assignmentsPath;
	private readonly SemanticKernelService _sk;

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	public AssignmentService(IWebHostEnvironment env, SemanticKernelService sk)
    {
        var dataRoot = Path.Combine(env.ContentRootPath, "SmartStudy.API", "Data");
        // Match on-disk filename casing to avoid issues on case-sensitive FS
        _assignmentsPath = Path.Combine(dataRoot, "Assignments.Json");
		_sk = sk;
    }

	public async Task<AssignmentDTO> AddAssignmentAsync(string studentId, string teacherId, string classId, string filePath, string teacherComment)
	{
		// Load existing assignments
		var assignments = await LoadAssignmentsAsync();

		// Generate a simple ID (a###). In production you'd use a GUID.
		string NextId()
		{
			// find max numeric suffix
			var max = 0;
			foreach (var a in assignments)
			{
				if (!string.IsNullOrEmpty(a.Id) && a.Id.Length > 1 && a.Id[0] == 'a')
				{
					if (int.TryParse(a.Id[1..], out var n))
						max = Math.Max(max, n);
				}
			}
			return $"a{(max + 1).ToString("D3")}";
		}


		var assignment = new AssignmentDTO
		{
			Id = NextId(),
			StudentId = studentId,
			TeacherId = teacherId,
			ClassId = classId,
			Title = Path.GetFileNameWithoutExtension(filePath) ?? "New Assignment",
			FileName = Path.GetFileName(filePath) ?? string.Empty,
			FilePath = filePath,
			TeacherComments = teacherComment,
		};

		// Extract text from PDF into a variable and store it
		string extractedText = string.Empty;
		try
		{
			Console.WriteLine("EXTRACTING TEXT FROM PDF...: " + filePath);
			extractedText = PDFPigService.ExtractText(filePath);
		}
		catch
		{
			Console.WriteLine("ERROR ERROR TEXT EXTRACTION FAILED");
			// Swallow exceptions to avoid breaking flow; keep text empty if extraction fails
			extractedText = string.Empty;
		}
		assignment.ExtractedText = extractedText;

		// Build prompt with escaped braces for JSON example
		string prompt = $"You are an expert at creating practice question sets for students based on study materials provided.\n" +
						"Given the following extracted text from a student's assignment, generate a set of practice questions that would help the student review and understand the material better.\n\n" +
						$"Extracted Text:\n {extractedText} \n\n" +
                        "Output ONLY valid JSON (no commentary) following exactly this structure and naming. Return only the JSON in a response that can be parsed using JSON.Deserialize, DONT PRINT OUT '''json ''' IN YOUR OUTPUT, JUST THE JSON ITSELF!!:\n" +
                        "{\n" +
                        "  \"test\": {\n" +
                        "    \"questions\": [\n" +
                        "      {\n" +
                        "        \"questionType\": \"multipleChoice\",\n" +
                        "        \"questionText\": \"What is the capital of France?\",\n" +
                        "        \"choices\": [\"London\", \"Berlin\", \"Paris\", \"Rome\"],\n" +
                        "        \"correctAnswer\": \"Paris\",\n" +
                        "        \"explanation\": \"Paris is the capital and largest city of France, situated on the river Seine.\"\n" +
                        "      },\n" +
                        "      {\n" +
                        "        \"questionType\": \"multipleChoice\",\n" +
                        "        \"questionText\": \"Which of the following is a primary color?\",\n" +
                        "        \"choices\": [\"Green\", \"Orange\", \"Blue\", \"Purple\"],\n" +
                        "        \"correctAnswer\": \"Blue\",\n" +
                        "        \"explanation\": \"The three primary colors are Red, Blue, and Yellow. Blue is the only primary color listed among the options.\"\n" +
                        "      },\n" +
                        "      {\n" +
                        "        \"questionType\": \"multipleChoice\",\n" +
                        "        \"questionText\": \"The Earth is flat.\",\n" +
                        "        \"choices\": [\"True\", \"False\"],\n" +
                        "        \"correctAnswer\": \"False\",\n" +
                        "        \"explanation\": \"The Earth is approximately spherical in shape, slightly flattened at the poles and bulging at the equator.\"\n" +
                        "      }\n" +
                        "    ]\n" +
                        "  }\n" +
                        "}\n\n" +
                        "Replace the example questions with new ones derived from the extracted text. Keep field names identical.";

		// Call the model with the prompt and capture the response (best-effort)
		string modelResp = string.Empty;
		try
		{
			var ext = Path.GetExtension(filePath)?.ToLowerInvariant();
			if (!string.IsNullOrWhiteSpace(extractedText) && ext == ".pdf")
			{
				modelResp = await _sk.PromptAsync(prompt);
			}
			else
			{
				Console.WriteLine("ERROR ERROR ERROR: Skipping model call - either non-PDF or no extracted text.");
				Console.WriteLine($"File extension: {ext}, Extracted text length: {extractedText.Length}");
				// Non-PDF or no text extracted; skip model call
				modelResp = "{\"test\":{\"questions\":[]}}";
			}
		}
		catch (Exception exModel)
		{
			Console.WriteLine("[AssignmentService] MODEL CALL FAILED: " + exModel.GetType().Name + ": " + exModel.Message);
			Console.WriteLine("[AssignmentService] StackTrace: " + exModel.StackTrace);
			// Keep going with empty questions to avoid breaking upload flow
			modelResp = "{\"test\":{\"questions\":[]}}";
		}

	var practiceSet = await PracticeSetsService.MakePracticeSetAsync(studentId: studentId, classId: classId, srcAssignmentId: assignment.Id, questions: modelResp);
        assignment.PracticeSetId = practiceSet.Id;

		// Append and persist
		assignments.Add(assignment);
		await SaveAssignmentsAsync(assignments);

		Console.WriteLine("ADDED ASSIGNMENT: " + assignment.Id);
		return assignment;
	}

	/// <summary>
	/// Returns a single assignment by its Id, or null if not found.
	/// </summary>
	public async Task<AssignmentDTO?> GetAssignmentByIdAsync(string assignmentId)
	{
		if (string.IsNullOrWhiteSpace(assignmentId)) return null;

		var assignments = await LoadAssignmentsAsync();
		return assignments.FirstOrDefault(a => a.Id.Equals(assignmentId, StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>
	/// Returns all assignments for a given student (userId) and class (classId).
	/// </summary>
	public async Task<List<AssignmentDTO>> GetAssignmentsAsync(string userId, string classId)
	{
		if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(classId))
			return new List<AssignmentDTO>();

		var assignments = await LoadAssignmentsAsync();
		return assignments
			.Where(a => a.StudentId.Equals(userId, StringComparison.OrdinalIgnoreCase)
					 && a.ClassId.Equals(classId, StringComparison.OrdinalIgnoreCase))
			.ToList();
	}

	private async Task<List<AssignmentDTO>> LoadAssignmentsAsync()
	{
		if (!File.Exists(_assignmentsPath)) return new();
		await using var stream = File.OpenRead(_assignmentsPath);
		return await JsonSerializer.DeserializeAsync<List<AssignmentDTO>>(stream, JsonOptions) ?? new();
	}

	private async Task SaveAssignmentsAsync(List<AssignmentDTO> items)
	{
		// Ensure directory exists
		Directory.CreateDirectory(Path.GetDirectoryName(_assignmentsPath)!);
		await using var stream = File.Create(_assignmentsPath);
		await JsonSerializer.SerializeAsync(stream, items, JsonOptions);
	}
}

