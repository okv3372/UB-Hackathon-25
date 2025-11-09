using System.Text.Json;
using Microsoft.AspNetCore.Components;
using SmartStudy.API.Services;
using SmartStudy.Models;
using SmartStudy.Services;
namespace SmartStudy.Components.Pages.Assignment;
public partial class Assignment : ComponentBase
{
    [Parameter]
    public string? UserId { get; set; }
    [Parameter]
    public string? ClassId { get; set; }
    [Parameter]
    public string? AssignmentId { get; set; }
    
    // Inject the AssignmentService to retrieve assignment details
    [Inject] private AssignmentService AssignmentService { get; set; } = default!;

    // Retrieved assignment instance (null if not found)
    public AssignmentDTO? CurrentAssignment { get; set; }
    // Retrieved practice set derived from the assignment (null if not found)
    public PracticeSetDTO? CurrentPracticeSet { get; set; }

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
  // Practice set JSON source (defaults to empty scaffold, replaced when CurrentPracticeSet available)
  private string mockJson = string.Empty;
    // Parsed test data
    public TestData? Test { get; set; }
  protected override async Task OnInitializedAsync()
  {
    // Fetch assignment and its practice set if an id was provided
    if (!string.IsNullOrWhiteSpace(AssignmentId))
    {
      CurrentAssignment = await AssignmentService.GetAssignmentByIdAsync(AssignmentId);
      CurrentPracticeSet = await PracticeSetsService.GetPracticeSetAsync(AssignmentId);
    }

    // If we have practice set questions, override mockJson
    if (!string.IsNullOrWhiteSpace(CurrentPracticeSet?.Questions))
    {
      mockJson = CurrentPracticeSet!.Questions;
    }

    // Parse whichever JSON we now have
    try
    {
      var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
      var pkg = JsonSerializer.Deserialize<TestPackage>(mockJson, opts);
      Test = pkg?.Test;
    }

    catch { Test = null; }
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