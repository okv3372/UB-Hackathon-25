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

    protected override Task OnInitializedAsync()
    {
        // Placeholder: future fetch logic using UserId, ClassId, AssignmentId
        return Task.CompletedTask;
    }
}
