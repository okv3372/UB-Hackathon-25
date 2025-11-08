using Microsoft.AspNetCore.Components;

namespace SmartStudy.Components.Pages.Student;

public partial class Student : ComponentBase
{
    [Parameter]
    public string? UserId { get; set; }

    [Parameter]
    public string? ClassId { get; set; }

    [Parameter]
    public string? StudentId { get; set; }

    // Placeholder state/logic for the Student page
    protected override Task OnInitializedAsync()
    {
        return Task.CompletedTask;
    }

    private void OnAssignmentClick(string assignmentId)
    {
        // TODO: implement behavior later (e.g., navigate or open details)
    }

    // Convenience handlers to avoid inline string quoting in Razor
    private void OnAssignment1() => OnAssignmentClick("assignmentId1");
    private void OnAssignment2() => OnAssignmentClick("assignmentId2");
    private void OnAssignment3() => OnAssignmentClick("assignmentId3");
}
