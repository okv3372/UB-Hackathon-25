 using Microsoft.AspNetCore.Components;

namespace SmartStudy.Components.Pages;

public partial class UserProfileCard : ComponentBase
{
    // Who the assignment page should be opened for (student id)
    [Parameter] public string? TargetUserId { get; set; }

    // Context of the class and which assignment to open
    [Parameter] public string? ClassId { get; set; }

    // (Optional) keep these if you also use them elsewhere
    [Parameter] public string? UserId { get; set; }   // current/logged-in user (not used for nav here)

    [Parameter] public string UserName { get; set; } = "No Name";
    [Parameter] public string UserTitle { get; set; } = string.Empty;
    [Parameter] public string ProfileImageUrl { get; set; } = "https://via.placeholder.com/90/F7C59F/004E89?text=👤";
    [Parameter] public string ProfileDetailPageUrl { get; set; } = "/userprofile";
    [Parameter] public string UserEmail { get; set; } = string.Empty;
    [Parameter] public string UserPhoneNumber { get; set; } = string.Empty;

    [Parameter] public EventCallback<UserProfileCard> OnViewProfile { get; set; }

    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

protected void OpenAssignmentsPage()
{
    // We need who (TargetUserId) and which class (ClassId)
    if (string.IsNullOrWhiteSpace(TargetUserId) || string.IsNullOrWhiteSpace(ClassId))
    {
        Console.WriteLine($"[UserProfileCard] Missing ids: TargetUserId='{TargetUserId}', ClassId='{ClassId}'");
        return;
    }

    // Use the logged-in user id in the route if you have it; fall back to "me"
    var current = string.IsNullOrWhiteSpace(UserId) ? "me" : UserId;

    // → /{userId}/Class/{classId}/student/{studentId}
    NavigationManager.NavigateTo($"/{current}/Class/{ClassId}/student/{TargetUserId}");
}


    protected async Task InvokeViewProfile()
    {
        if (OnViewProfile.HasDelegate)
            await OnViewProfile.InvokeAsync(this);
        else
            NavigationManager.NavigateTo(ProfileDetailPageUrl);
    }

    protected void OpenUploadModal()
    {
        if (!string.IsNullOrWhiteSpace(UserPhoneNumber))
            NavigationManager.NavigateTo($"tel:{UserPhoneNumber}", forceLoad: true);
    }
}
