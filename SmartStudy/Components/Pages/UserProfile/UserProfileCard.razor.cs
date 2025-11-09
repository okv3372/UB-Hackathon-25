using Microsoft.AspNetCore.Components;
using SmartStudy.Components.Shared;
using SmartStudy.Services;

namespace SmartStudy.Components.Pages;

public partial class UserProfileCard : ComponentBase
{
    private const string DefaultProfileImageUrl = "/profile_pictures/default-profile-account-unknown-icon-black-silhouette-free-vector.jpg";

    [Parameter] public string? TargetUserId { get; set; }
    [Parameter] public string? ClassId { get; set; }

    [Parameter] public string? UserId { get; set; }

    [Parameter] public string UserName { get; set; } = "No Name";
    [Parameter] public string UserTitle { get; set; } = string.Empty;
    [Parameter] public string ProfileImageUrl { get; set; } = DefaultProfileImageUrl;
    [Parameter] public string ProfileDetailPageUrl { get; set; } = "/userprofile";
    [Parameter] public string UserEmail { get; set; } = string.Empty;
    [Parameter] public string UserPhoneNumber { get; set; } = string.Empty;

    [Parameter] public EventCallback<UserProfileCard> OnViewProfile { get; set; }

    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    protected string ResolvedProfileImageUrl { get; private set; } = DefaultProfileImageUrl;

    private UploadAssignmentModal? uploadModal;

    protected override void OnParametersSet()
    {
        ResolvedProfileImageUrl = ResolveProfileImageUrl();
    }

    private string ResolveProfileImageUrl()
    {
        if (!string.IsNullOrWhiteSpace(TargetUserId))
        {
            var profile = ProfileService.GetProfile(TargetUserId);
            if (!string.IsNullOrWhiteSpace(profile?.PictureUrl))
            {
                return profile.PictureUrl!;
            }
        }

        if (!string.IsNullOrWhiteSpace(ProfileImageUrl))
        {
            return ProfileImageUrl;
        }

        return DefaultProfileImageUrl;
    }

    private void ShowUploadModal()
    {
        uploadModal?.OpenModal();
    }

    private Task OnAssignmentUploaded(SmartStudy.Models.AssignmentDTO dto)
    {
        Console.WriteLine($"[UserProfileCard] Assignment uploaded: Id={dto.Id}, Title={dto.Title}, File={dto.FileName}, Path={dto.FilePath}");
        return Task.CompletedTask;
    }

    protected void OpenAssignmentsPage()
    {
        if (string.IsNullOrWhiteSpace(TargetUserId) || string.IsNullOrWhiteSpace(ClassId))
        {
            Console.WriteLine($"[UserProfileCard] Missing ids: TargetUserId='{TargetUserId}', ClassId='{ClassId}'");
            return;
        }

        var current = string.IsNullOrWhiteSpace(UserId) ? "me" : UserId;

        // Navigates to /{userId}/Class/{classId}/student/{studentId}
        NavigationManager.NavigateTo($"/{current}/Class/{ClassId}/student/{TargetUserId}");
    }

    protected async Task InvokeViewProfile()
    {
        if (OnViewProfile.HasDelegate)
        {
            await OnViewProfile.InvokeAsync(this);
        }
        else
        {
            NavigationManager.NavigateTo(ProfileDetailPageUrl);
        }
    }

    protected void OpenUploadModal()
    {
        if (!string.IsNullOrWhiteSpace(UserPhoneNumber))
        {
            NavigationManager.NavigateTo($"tel:{UserPhoneNumber}", forceLoad: true);
        }
    }
}
