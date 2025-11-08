using Microsoft.AspNetCore.Components;

namespace SmartStudy.Components.Pages.ClassList;

public partial class ClassList : ComponentBase
{
    [Parameter]
    public string? UserId { get; set; }

    // Placeholder state/logic for the Class List page
    protected override Task OnInitializedAsync()
    {
        return Task.CompletedTask;
    }

    [Inject]
    private NavigationManager Nav { get; set; } = default!;

    private void NavigateToClass()
    {
        if (string.IsNullOrWhiteSpace(UserId))
        {
            return; // no user id context
        }
        // TODO: Change hardcoded class ID so it gets it from the list of class IDs
        var target = $"/{UserId}/Class/classId1"; 
        Nav.NavigateTo(target);
    }
}
