using Microsoft.AspNetCore.Components;
using SmartStudy.API.Services;   // ✅ Add this
using SmartStudy.Models;        // ✅ For UserDTO

namespace SmartStudy.Components.Pages.Class;

public partial class Class : ComponentBase
{
    [Inject] 
    public UsersService UsersService { get; set; } = default!; // ✅ Add this

    [Parameter]
    public string? UserId { get; set; }

    [Parameter]
    public string? ClassId { get; set; }

    // ✅ This is the flag your .razor page will use
    protected bool isStudent { get; set; }

    // Modal reference and selected profile display data
    private ProfileModal? profileModalRef;
    protected string? SelectedUserName { get; set; }
    protected string? SelectedProfileImageUrl { get; set; }
    protected string? SelectedUserInterests { get; set; }
    protected string? SelectedTitle { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrWhiteSpace(UserId))
        {
            var user = await UsersService.GetUserByIdAsync(UserId);

            // ✅ Teacher → isStudent = false | Student → isStudent = true
            isStudent = !string.Equals(user?.Role, "Teacher", StringComparison.OrdinalIgnoreCase);
        }

        await base.OnInitializedAsync();
    }

    protected void OnStudentViewProfile(UserProfileCard card)
    {
        SelectedUserName = card.UserName;
        SelectedTitle = card.UserTitle;
        SelectedProfileImageUrl = card.ProfileImageUrl;
        SelectedUserInterests = "Reading, learning, and collaborating.";
        profileModalRef?.OpenModal();
    }

    protected void OpenBottomProfileModal()
    {
        SelectedUserName = SelectedUserName ?? "Class Member";
        SelectedTitle = SelectedTitle ?? "Student";
        SelectedProfileImageUrl = SelectedProfileImageUrl ?? "/favicon.png";
        SelectedUserInterests = SelectedUserInterests ?? "No interests provided.";
        profileModalRef?.OpenModal();
    }
}
