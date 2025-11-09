using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Linq;

namespace SmartStudy.Components.Pages.Class;

public partial class ProfileModal : ComponentBase
{
    // Visibility & state
    private bool showModal = false;
    private bool isEditing = false;

    // Parameters provided by parent
    [Parameter] public string? UserName { get; set; }
    [Parameter] public string? ProfilePictureUrl { get; set; }
    [Parameter] public string? UserInterests { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public bool EnableEditButton { get; set; } = false;
    [Parameter] public string? GuardianName { get; set; }
    [Parameter] public string? GuardianEmail { get; set; }

    private string ResolvedPictureUrl => string.IsNullOrWhiteSpace(ProfilePictureUrl) ? "/favicon.png" : ProfilePictureUrl!;
    private string DisplayTitle => !string.IsNullOrWhiteSpace(Title)
        ? Title!
        : (!string.IsNullOrWhiteSpace(UserName) ? UserName! : "Profile");

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

    private void BackdropClick(MouseEventArgs _) => CloseModal();

    private void EnableEditing() => isEditing = true;
    private void CancelEditing() => isEditing = false;

    private void SaveProfile()
    {
        // TODO: Persist changes if needed.
        isEditing = false;
    }

    private IEnumerable<string> SplitInterests(string? interests) => string.IsNullOrWhiteSpace(interests)
        ? Enumerable.Empty<string>()
        : interests.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape") CloseModal();
    }
}
