using Microsoft.AspNetCore.Components;

namespace SmartStudy.Components.Pages.Login;

public partial class Login : ComponentBase
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    protected string? UserIdInput { get; set; }

    protected void GoToClassList()
    {
        var userId = string.IsNullOrWhiteSpace(UserIdInput) ? "" : UserIdInput.Trim();
        if (!string.IsNullOrEmpty(userId))
        {
            Navigation.NavigateTo($"/{userId}/ClassList");
        }
    }
    // Placeholder state/logic for the Login page
    protected override Task OnInitializedAsync()
    {
        return Task.CompletedTask;
    }
}
