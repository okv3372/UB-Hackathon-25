// Assignment.cs
using System.Text.Json;
using Microsoft.AspNetCore.Components;
namespace SmartStudy.Components.Pages.Assignment;
public partial class Assignment : ComponentBase
{
    [Parameter]
    public string? UserId { get; set; }
    [Parameter]
    public string? ClassId { get; set; }
    [Parameter]
    public string? AssignmentId { get; set; }
    // Models for the test JSON
    public class TestPackage
    {
        public TestData? Test { get; set; }
    }
    public class TestData
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public List<QuestionData>? Questions { get; set; }
    }
    public class QuestionData
    {
        public string? Id { get; set; }
        public string? QuestionType { get; set; }
        public string? QuestionText { get; set; }
        public List<string>? Choices { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? Explanation { get; set; }
        // UI state
        public string? SelectedChoice { get; set; }
        public bool? IsCorrect { get; set; } // null => not checked, true/false => checked
        public bool ShowExplanation { get; set; } = false;
    }
    // Hard-coded mock JSON (will be replaced later by backend)
    private readonly string mockJson = @"
{
  ""test"": {
    ""id"": ""test123"",
    ""title"": ""Sample Practice Test"",
    ""questions"": [
      {
        ""id"": ""q1"",
        ""questionType"": ""multipleChoice"",
        ""questionText"": ""What is the capital of France?"",
        ""choices"": [
          ""London sjdfbwhrb fsrbflh dbgsbdgbsdgbs lhgbjebghwrb gbegwergfb wrhfgle gfjhwerbgjw herbgjqeb"",
          ""Berlin"",
          ""Paris"",
          ""Rome""
        ],
        ""correctAnswer"": ""Paris"",
        ""explanation"": ""Paris is the capital and largest city of France, situated on the river Seine.""
      },
      {
        ""id"": ""q2"",
        ""questionType"": ""multipleChoice"",
        ""questionText"": ""Which of the following is a primary color?"",
        ""choices"": [
          ""Green"",
          ""Orange"",
          ""Blue"",
          ""Purple""
        ],
        ""correctAnswer"": ""Blue"",
        ""explanation"": ""The three primary colors are Red, Blue, and Yellow. Blue is the only primary color listed among the options.""
      },
      {
        ""id"": ""q3"",
        ""questionType"": ""trueFalse"",
        ""questionText"": ""The Earth is flat."",
        ""choices"": [
          ""True"",
          ""False""
        ],
        ""correctAnswer"": ""False"",
        ""explanation"": ""The Earth is approximately spherical in shape, slightly flattened at the poles and bulging at the equator.""
      }
    ]
  }
}
";
    // Parsed test data
    public TestData? Test { get; set; }
    protected override Task OnInitializedAsync()
    {
        // Parse the mock JSON into our model
        try
        {
            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var pkg = JsonSerializer.Deserialize<TestPackage>(mockJson, opts);
            Test = pkg?.Test;
        }
        catch
        {
            Test = null;
        }
        // placeholder: future fetch logic using UserId, ClassId, AssignmentId
        return Task.CompletedTask;
    }
    // Action methods used by the UI (bound from razor)
    public void SelectChoice(QuestionData q, string choice)
    {
        q.SelectedChoice = choice;
        // reset previous check result when user changes choice
        q.IsCorrect = null;
        q.ShowExplanation = false;
    }
    public void CheckAnswer(QuestionData q)
    {
        if (q == null) return;
        if (q.SelectedChoice == null)
        {
            // no selection -> treat as incorrect but mark as checked
            q.IsCorrect = false;
            q.ShowExplanation = true;
            return;
        }
        q.IsCorrect = string.Equals(q.SelectedChoice, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
        q.ShowExplanation = true;
    }
    public void ResetQuestion(QuestionData q)
    {
        q.SelectedChoice = null;
        q.IsCorrect = null;
        q.ShowExplanation = false;
    }
    public void CheckAll()
    {
        if (Test?.Questions == null) return;
        foreach (var q in Test.Questions)
        {
            CheckAnswer(q);
        }
    }
    public void ResetAll()
    {
        if (Test?.Questions == null) return;
        foreach (var q in Test.Questions)
        {
            ResetQuestion(q);
        }
    }
}