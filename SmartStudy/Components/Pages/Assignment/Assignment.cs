using System;
using System.Collections.Generic;
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
        public QuestionData? Question { get; set; }
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
  private class LegacyTestPackage
  {
      public LegacyTestData? Test { get; set; }
  }
  private class LegacyTestData
  {
      public string? Id { get; set; }
      public string? Title { get; set; }
      public List<QuestionData>? Questions { get; set; }
  }
    // Practice set JSON source (defaults to empty scaffold, replaced when CurrentPracticeSet available)
  private string mockJson = string.Empty;
    // Parsed test data
    public TestData? Test { get; set; }
    private bool IsRegenerating { get; set; }
    private string? RegenerateError { get; set; }
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

    Test = DeserializeTestData(mockJson);
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
      var question = Test?.Question;
      if (question == null) return;
      CheckAnswer(question);
  }
  public void ResetAll()
  {
      var question = Test?.Question;
      if (question == null) return;
      ResetQuestion(question);
  }
  private TestData? DeserializeTestData(string? questionsJson)
  {
      if (string.IsNullOrWhiteSpace(questionsJson)) return null;

      try
      {
          var cleaned = SanitizeQuestionPayload(questionsJson);
          if (string.IsNullOrWhiteSpace(cleaned))
          {
              return new TestData();
          }

          var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
          var pkg = JsonSerializer.Deserialize<TestPackage>(cleaned, opts);
          if (pkg?.Test != null && cleaned.Contains("\"question\"", StringComparison.OrdinalIgnoreCase))
          {
              return pkg.Test;
          }

          var legacy = JsonSerializer.Deserialize<LegacyTestPackage>(cleaned, opts);
          var legacyTest = legacy?.Test;
          if (legacyTest == null) return null;

          if (legacyTest.Questions != null && legacyTest.Questions.Count > 0)
          {
              return new TestData
              {
                  Id = legacyTest.Id,
                  Title = legacyTest.Title,
                  Question = legacyTest.Questions[0]
              };
          }

          return new TestData
          {
              Id = legacyTest.Id,
              Title = legacyTest.Title,
              Question = null
          };
      }
      catch
      {
          return null;
      }
  }
  private static string SanitizeQuestionPayload(string raw)
  {
      if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

      var cleaned = raw.Trim();
      if (cleaned.StartsWith("```", StringComparison.Ordinal))
      {
          var firstLineEnd = cleaned.IndexOf('\n');
          if (firstLineEnd >= 0)
          {
              cleaned = cleaned[(firstLineEnd + 1)..];
          }

          var closingFence = cleaned.LastIndexOf("```", StringComparison.Ordinal);
          if (closingFence >= 0)
          {
              cleaned = cleaned[..closingFence];
          }
      }

      return cleaned.Trim();
  }

  private async Task LoadNewQuestionsAsync()
  {
      if (string.IsNullOrWhiteSpace(AssignmentId)) return;

      RegenerateError = null;
      IsRegenerating = true;
      await InvokeAsync(StateHasChanged);

      try
      {
          var practiceSet = await AssignmentService.RegeneratePracticeSetAsync(AssignmentId);
          if (practiceSet == null)
          {
              RegenerateError = "Unable to generate a new question right now.";
          }
          else
          {
              var parsed = DeserializeTestData(practiceSet.Questions);
              if (parsed == null)
              {
                  RegenerateError = "Received invalid question data.";
              }
              else
              {
                  CurrentPracticeSet = practiceSet;
                  mockJson = practiceSet.Questions ?? string.Empty;
                  Test = parsed;
                  if (parsed.Question == null)
                  {
                      RegenerateError = "Received invalid question data.";
                  }
              }
          }
      }
      catch (Exception ex)
      {
          RegenerateError = "Failed to load new question.";
          Console.WriteLine("[Assignment] LoadNewQuestionsAsync failed: " + ex.Message);
      }
      finally
      {
          IsRegenerating = false;
          await InvokeAsync(StateHasChanged);
      }
  }
}
