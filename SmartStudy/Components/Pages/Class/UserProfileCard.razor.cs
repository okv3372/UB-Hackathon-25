using Microsoft.AspNetCore.Components; // Required for [Parameter], [Inject], ComponentBase
namespace SmartStudy.Components.Pages;

public partial class UserProfileCard : ComponentBase
{
    // Reference to the ProfileModal component to control it from this card
    private Components.Pages.Class.ProfileModal? profileModalRef;
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

    protected void NavigateToAssignments()
    {
        NavigationManager.NavigateTo(ProfileDetailPageUrl);
    }

    protected void OpenProfileModal()
    {
        // Open the embedded profile modal instead of navigating
        profileModalRef?.OpenModal();
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