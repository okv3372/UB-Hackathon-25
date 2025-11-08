using Microsoft.AspNetCore.Components; // Required for [Parameter], [Inject], ComponentBase
namespace SmartStudy.Components.Pages;

public partial class UserProfileCard : ComponentBase
{
    // Event emitted to parent when user wants to view profile
    [Parameter]
    public EventCallback<UserProfileCard> OnViewProfile { get; set; }
    
    // Context identifiers needed to navigate to assignment page
    [Parameter]
    public string? UserId { get; set; }
    [Parameter]
    public string? ClassId { get; set; }
    // Default placeholder assignment id; parent can override
    [Parameter]
    public string AssignmentId { get; set; } = "assignmentId1";
    [Parameter]
    public string UserName { get; set; } = "No Name";

    [Parameter]
    public string UserTitle { get; set; } = string.Empty; // Optional title/role

    [Parameter]
    public string ProfileImageUrl { get; set; } = "https://via.placeholder.com/90/F7C59F/004E89?text=👤"; // Default avatar

    [Parameter]
    public string ProfileDetailPageUrl { get; set; } = "/userprofile"; // URL for the 'View Profile' button

    [Parameter]
    public string UserEmail { get; set; } = string.Empty; // For the email button

    [Parameter]
    public string UserPhoneNumber { get; set; } = string.Empty; // For the call button

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    protected void OpenAssignmentsPage()
    {
        // Ensure required route segments are present before navigating
        if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(ClassId) || string.IsNullOrWhiteSpace(AssignmentId))
        {
            // Could log or show a message; for now just do nothing if data incomplete
            return;
        }
        var targetUrl = $"/{UserId}/Class/{ClassId}/assignment/{AssignmentId}";
        NavigationManager.NavigateTo(targetUrl);
    }

    protected async Task InvokeViewProfile()
    {
        if (OnViewProfile.HasDelegate)
        {
            await OnViewProfile.InvokeAsync(this);
        }
        else
        {
            // Fallback: navigate to detail page if no modal wiring
            NavigationManager.NavigateTo(ProfileDetailPageUrl);
        }
    }

    protected void OpenUploadModal()
    {
        if (!string.IsNullOrWhiteSpace(UserPhoneNumber))
        {
            // Opens default phone dialer (might not work on all browsers/OS)
            NavigationManager.NavigateTo($"tel:{UserPhoneNumber}", forceLoad: true);
        }
        // You might add an alert or some feedback if phone is not available
    }
}