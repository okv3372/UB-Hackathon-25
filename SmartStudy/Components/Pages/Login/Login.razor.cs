using Microsoft.AspNetCore.Components;
// adjust this using to wherever UsersService actually lives:
using SmartStudy.API.Services; 
// using SmartStudy.Services; // use this instead if you moved it

namespace SmartStudy.Components.Pages.Login;

public partial class Login : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private UsersService Users { get; set; } = default!;

    protected string? UserIdInput { get; set; }
    protected string? PasswordInput { get; set; }

    protected bool IsSubmitting { get; set; }
    protected string? Error { get; set; }

    protected override Task OnInitializedAsync() => Task.CompletedTask;

    protected async Task HandleLoginAsync()
    {
        Error = null;

        var username = (UserIdInput ?? string.Empty).Trim();
        var password = PasswordInput ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Error = "Please enter both username and password.";
            return;
        }

        try
        {
            IsSubmitting = true;

            var user = await Users.ValidateLoginAsync(username, password);
            if (user is not null)
            {
                // Navigate to that user's class list (using their Id from the JSON)
                Navigation.NavigateTo($"/{user.Id}/ClassList");
                return;
            }

            Error = "Invalid username or password.";
        }
        catch (Exception)
        {
            Error = "Something went wrong while signing in. Please try again.";
        }
        finally
        {
            IsSubmitting = false;
            PasswordInput = string.Empty; // clear pw on failure
        }
    }
}
