using System.Text.Json;
using SmartStudy.Models;
using SmartStudy.Services;
using SmartStudy.API.SemanticKernel;

namespace SmartStudy.API.Services;

public class AssignmentService
{
	private readonly string _assignmentsPath;
	private readonly SemanticKernelService _sk;
	private const string EmptyQuestionsJson = "{\"test\":{\"question\":null}}";

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

	public async Task<AssignmentDTO> AddAssignmentAsync(string studentId, string teacherId, string classId, string filePath, string teacherComment, string title)
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
			Title = title,
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

		// Retrieve student profile via ProfileService
		ProfileDTO? profile = ProfileService.GetProfile(studentId);
		string profileBio = profile?.Bio ?? string.Empty;

		string prompt = BuildPracticeQuestionsPrompt(extractedText, teacherComment, profileBio);

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
				modelResp = EmptyQuestionsJson;
			}
		}
		catch (Exception exModel)
		{
			Console.WriteLine("[AssignmentService] MODEL CALL FAILED: " + exModel.GetType().Name + ": " + exModel.Message);
			Console.WriteLine("[AssignmentService] StackTrace: " + exModel.StackTrace);
			// Keep going with empty questions to avoid breaking upload flow
			modelResp = EmptyQuestionsJson;
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

	public async Task<PracticeSetDTO?> RegeneratePracticeSetAsync(string assignmentId)
	{
		if (string.IsNullOrWhiteSpace(assignmentId)) return null;

		var assignments = await LoadAssignmentsAsync();
		var assignment = assignments.FirstOrDefault(a => a.Id.Equals(assignmentId, StringComparison.OrdinalIgnoreCase));
		if (assignment == null) return null;

		var profile = ProfileService.GetProfile(assignment.StudentId);
		string profileBio = profile?.Bio ?? string.Empty;
		string prompt = BuildPracticeQuestionsPrompt(assignment.ExtractedText ?? string.Empty, assignment.TeacherComments ?? string.Empty, profileBio);

		string modelResp = EmptyQuestionsJson;
		try
		{
			var ext = Path.GetExtension(assignment.FilePath)?.ToLowerInvariant();
			if (!string.IsNullOrWhiteSpace(assignment.ExtractedText) && ext == ".pdf")
			{
				modelResp = await _sk.PromptAsync(prompt);
			}
			else
			{
				Console.WriteLine($"[AssignmentService] Skipping regenerate model call - ext {ext}, text length {assignment.ExtractedText?.Length ?? 0}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("[AssignmentService] REGENERATE MODEL CALL FAILED: " + ex.GetType().Name + ": " + ex.Message);
		}

		var existingPracticeSet = !string.IsNullOrWhiteSpace(assignment.PracticeSetId)
			? await PracticeSetsService.GetPracticeSetAsync(assignment.Id)
			: null;

		PracticeSetDTO practiceSet;
		if (existingPracticeSet != null)
		{
			existingPracticeSet.Questions = modelResp;
			existingPracticeSet.StudentId = assignment.StudentId;
			existingPracticeSet.ClassId = assignment.ClassId;
			existingPracticeSet.SrcAssignmentId = assignment.Id;
			practiceSet = existingPracticeSet;
			await PracticeSetsService.SavePracticeSetAsync(practiceSet);
		}
		else
		{
			if (string.IsNullOrWhiteSpace(assignment.PracticeSetId))
			{
				practiceSet = await PracticeSetsService.MakePracticeSetAsync(assignment.StudentId, assignment.ClassId, assignment.Id, modelResp);
				assignment.PracticeSetId = practiceSet.Id;
			}
			else
			{
				practiceSet = new PracticeSetDTO
				{
					Id = assignment.PracticeSetId,
					StudentId = assignment.StudentId,
					ClassId = assignment.ClassId,
					SrcAssignmentId = assignment.Id,
					Questions = modelResp,
					Notes = string.Empty
				};
				await PracticeSetsService.SavePracticeSetAsync(practiceSet);
			}
		}

		await SaveAssignmentsAsync(assignments);
		Console.WriteLine("[AssignmentService] Regenerated practice set for assignment: " + assignment.Id);
		return practiceSet;
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

	private static string BuildPracticeQuestionsPrompt(string extractedText, string teacherComment, string profileBio)
	{
		extractedText ??= string.Empty;
		teacherComment ??= string.Empty;
		profileBio ??= string.Empty;

		return $"You are an expert at creating targeted practice question prompts for students based on study materials provided.\n" +
			   "Given the following extracted text from a student's graded assignment, generate a single practice question that would help the student review and understand the material better.\n\n" +
			   $"Extracted Text:\n {extractedText} \n\n" +
			   "Read the extracted text and determine which of the users answers were wrong and make your practice question so that it will help with the users weak areas. " +
			   $"Also consider this addition context about the assignment when making your question: {teacherComment}\n\n" +
			   $"Also cater the question based on the users interests, provided here is a bio desribing what the users intrested in, generte the question related to this: {profileBio}\n\n" +
			   "ALWATS give exactly 1 question, make sure the question is possibe to answer given all the context in the prompt. never mention any names from either the profile or the extracted text in any question. for math-focused material, you may choose either a word-based problem or a direct numeric expression, but ensure it can be solved from the provided context." +
			   "Output ONLY valid JSON (no commentary) following exactly this structure and naming. Return only the JSON in a response that can be parsed using JSON.Deserialize, DONT PRINT OUT '''json ''' IN YOUR OUTPUT, JUST THE JSON ITSELF!!:\n" +
			   "DO NOT ADD MARKDOWN FORMATTING OR BAD THINGS WILL HAPPEN\n\n" +
			   "{\n" +
			   "  \"test\": {\n" +
			   "    \"question\": {\n" +
			   "      \"questionType\": \"multipleChoice\",\n" +
			   "      \"questionText\": \"What is the capital of France?\",\n" +
			   "      \"choices\": [\"London\", \"Berlin\", \"Paris\", \"Rome\"],\n" +
			   "      \"correctAnswer\": \"Paris\",\n" +
			   "      \"explanation\": \"Paris is the capital and largest city of France, situated on the river Seine.\"\n" +
			   "    }\n" +
			   "  }\n" +
			   "}\n\n" +
			   "Replace the example question with a new one derived from the extracted text. Keep field names identical." +
			   "DONT PRINT OUT '''json ''' IN YOUR OUTPUT, JUST THE JSON ITSELF!!";
	}
}
