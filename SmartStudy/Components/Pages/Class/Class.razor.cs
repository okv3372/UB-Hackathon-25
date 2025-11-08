using Microsoft.AspNetCore.Components;

namespace SmartStudy.Components.Pages.Class;

public partial class Class : ComponentBase
{
	[Parameter]
	public string? UserId { get; set; }

	[Parameter]
	public string? ClassId { get; set; }

	// Modal reference and selected profile display data
	private ProfileModal? profileModalRef;
	protected string? SelectedUserName { get; set; }
	protected string? SelectedProfileImageUrl { get; set; }
	protected string? SelectedUserInterests { get; set; }
	protected string? SelectedTitle { get; set; }

	protected override Task OnInitializedAsync()
	{
		// Placeholder for future data fetch using UserId & ClassId
		return Task.CompletedTask;
	}

	protected void OnStudentViewProfile(UserProfileCard card)
	{
		// Pull basic info from the card's public parameters
		SelectedUserName = card.UserName;
		SelectedTitle = card.UserTitle;
		SelectedProfileImageUrl = card.ProfileImageUrl;
		// Interests are not present on the card; use a placeholder for now
		SelectedUserInterests = "Reading, learning, and collaborating.";
		profileModalRef?.OpenModal();
	}

	protected void OpenBottomProfileModal()
	{
		// If no selection yet, open a generic profile modal content
		SelectedUserName = SelectedUserName ?? "Class Member";
		SelectedTitle = SelectedTitle ?? "Student";
		SelectedProfileImageUrl = SelectedProfileImageUrl ?? "/favicon.png";
		SelectedUserInterests = SelectedUserInterests ?? "No interests provided.";
		profileModalRef?.OpenModal();
	}
}

