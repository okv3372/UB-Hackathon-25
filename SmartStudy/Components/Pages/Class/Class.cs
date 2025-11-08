using Microsoft.AspNetCore.Components;

namespace SmartStudy.Components.Pages.Class;

public partial class Class : ComponentBase
{
	[Parameter]
	public string? UserId { get; set; }

	[Parameter]
	public string? ClassId { get; set; }

	protected override Task OnInitializedAsync()
	{
		// Placeholder for future data fetch using UserId & ClassId
		return Task.CompletedTask;
	}
}

