using Microsoft.AspNetCore.Components;
using SmartStudy.API.Services;
using SmartStudy.Models;

namespace SmartStudy.Components.Pages.Student;

public partial class Student : ComponentBase
{
    // Route params from @page directive
    [Parameter] public string UserId { get; set; } = default!;
    [Parameter] public string ClassId { get; set; } = default!;
    [Parameter] public string StudentId { get; set; } = default!;

    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected AssignmentService AssignmentService { get; set; } = default!;

    // Real assignments list populated from AssignmentService
    public List<AssignmentDTO>? Assignments { get; private set; }

    protected override async Task OnParametersSetAsync()
    {
        // Load assignments for this student & class
        Assignments = await AssignmentService.GetAssignmentsAsync(StudentId, ClassId);
    }

    protected string BuildDetailUrl(string assignmentId)
        => $"/{UserId}/Class/{ClassId}/assignment/{assignmentId}?studentId={StudentId}";
}
