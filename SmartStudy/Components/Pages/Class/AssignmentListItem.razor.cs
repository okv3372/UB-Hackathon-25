using Microsoft.AspNetCore.Components;
namespace SmartStudy.Components.Pages;

// Match the .razor filename and mark partial so Blazor links them.
public partial class AssignmentListItem : ComponentBase
{
    [Parameter]
    public string AssignmentName { get; set; } = "Untitled Assignment";

    [Parameter]
    public string GradedFileName { get; set; } = "No file available";

    // Grade parameter removed from UI; previously displayed grading info.

    // Optional: If you want the parent to handle navigation instead
    [Parameter] public EventCallback<string> OnViewDetails { get; set; }

    [Parameter] public string DetailPageUrl { get; set; } = "/assignmentdetails"; // Default URL

    // Inject NavigationManager for programmatic navigation
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    // Method to handle the button click and navigate
    protected void NavigateToDetails()
    {
        NavigationManager.NavigateTo(DetailPageUrl);

        // If using the OnViewDetails callback instead of direct navigation:
        // OnViewDetails.InvokeAsync(AssignmentName);
    }
}