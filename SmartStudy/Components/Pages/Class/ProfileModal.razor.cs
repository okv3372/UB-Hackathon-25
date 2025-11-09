using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Linq;
using SmartStudy.Services;
using SmartStudy.Models;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace SmartStudy.Components.Pages.Class;

public partial class ProfileModal : ComponentBase
{
    // Visibility & state
    private bool showModal = false;
    private bool isEditing = false;

    // Parameters provided by parent
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? ProfilePictureUrl { get; set; }
    [Parameter] public string? UserBio { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public bool EnableEditButton { get; set; } = false;
    [Parameter] public string? GuardianName { get; set; }
    [Parameter] public string? GuardianEmail { get; set; }
    [Parameter] public string? StudentId { get; set; }
    [Parameter] public string? GradeLevel { get; set; }
    public string? fallbackProfileImageUrl { get; set; } = "/Users/olivervarney/Desktop/SmartStudy/UB-Hackathon-25/SmartStudy/wwwroot/profile_pictures/default-profile-account-unknown-icon-black-silhouette-free-vector.jpg";

    private string ResolvedPictureUrl => string.IsNullOrWhiteSpace(ProfilePictureUrl) ? "/favicon.png" : ProfilePictureUrl!;
    private string DisplayTitle => !string.IsNullOrWhiteSpace(Title)
        ? Title!
        : (!string.IsNullOrWhiteSpace(Name) ? Name! : "Profile");

    // Control methods exposed to parent
    public void OpenModal()
    {
        showModal = true;
        isEditing = false;
        StateHasChanged();
    }

    public void CloseModal()
    {
        showModal = false;
        isEditing = false;
        StateHasChanged();
    }

    // New: Open the modal by loading details for a given user id.
    public void OpenForUser(ProfileDTO profile)
    {
        Name = profile.Name;
        ProfilePictureUrl = !string.IsNullOrWhiteSpace(profile?.PictureUrl)
            ? profile!.PictureUrl
            : fallbackProfileImageUrl;

        Title = !string.IsNullOrWhiteSpace(profile?.GradeLevel)
            ? $"Grade {profile!.GradeLevel}": "";
        GradeLevel = profile?.GradeLevel;
        StudentId = profile?.StudentId;

        UserBio = !string.IsNullOrWhiteSpace(profile?.Bio)
            ? profile!.Bio
            : "No profile details available.";

        GuardianName = profile?.GuardianName ?? string.Empty;
        GuardianEmail = profile?.GuardianEmail ?? string.Empty;

        OpenModal();
    }

    private void BackdropClick(MouseEventArgs _) => CloseModal();

    private void EnableEditing() => isEditing = true;
    private void CancelEditing() => isEditing = false;

    private void SaveProfile()
    {
        // Persist changes via ProfileService helper, using StudentId as key
        var updated = ProfileService.UpdateProfileFromValues(
            StudentId,
            ProfilePictureUrl,
            Name,
            UserBio,
            GradeLevel,
            GuardianName,
            GuardianEmail);

        // Refresh UI with saved values
        if (updated != null)
        {
            StudentId = updated.StudentId;
            ProfilePictureUrl = updated.PictureUrl;
            Name = updated.Name;
            UserBio = updated.Bio;
            GradeLevel = updated.GradeLevel;
            GuardianName = updated.GuardianName;
            GuardianEmail = updated.GuardianEmail;
        }

        isEditing = false;
        StateHasChanged();
    }

    private IEnumerable<string> SplitInterests(string? interests) => string.IsNullOrWhiteSpace(interests)
        ? Enumerable.Empty<string>()
        : interests.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape") CloseModal();
    }
}
