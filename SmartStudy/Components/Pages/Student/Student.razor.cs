using Microsoft.AspNetCore.Components;

namespace SmartStudy.Components.Pages.Student;

public partial class Student : ComponentBase
{
    // Route params from @page directive
    [Parameter] public string UserId { get; set; } = default!;
    [Parameter] public string ClassId { get; set; } = default!;
    [Parameter] public string StudentId { get; set; } = default!;

    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    // Replace with your real service/model as needed
    public List<AssignmentVM>? Assignments { get; private set; }

    protected override async Task OnParametersSetAsync()
    {
        // TODO: swap to your real data call:
        // Assignments = await _assignmentService.GetForStudentAsync(ClassId, StudentId);
        await Task.Yield();
        Assignments = new()
            {
                new() { Id="assignmentId1", Name="Intro Worksheet", GradedFile="intro_worksheet.pdf" },
                new() { Id="assignmentId2", Name="Reading Quiz 1",   GradedFile="quiz1_results.pdf" },
                new() { Id="assignmentId3", Name="Project Proposal", GradedFile="proposal_feedback.docx" },
            };
    }

    protected string BuildDetailUrl(string assignmentId)
        => $"/{UserId}/Class/{ClassId}/assignment/{assignmentId}?studentId={StudentId}";

    public sealed class AssignmentVM
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string GradedFile { get; set; } = "";
    }
}
