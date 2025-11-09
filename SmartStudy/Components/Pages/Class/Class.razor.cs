using Microsoft.AspNetCore.Components;
using SmartStudy.API.Services;
using SmartStudy.Models;

namespace SmartStudy.Components.Pages.Class;

public partial class Class : ComponentBase
{
    [Inject] public UsersService UsersService { get; set; } = default!;
    [Inject] public EnrollmentService EnrollmentService { get; set; } = default!;

    [Parameter] public string? UserId { get; set; }
    [Parameter] public string? ClassId { get; set; }
    [Inject] public AssignmentService AssignmentService { get; set; } = default!;

    protected bool isStudent { get; set; }

    // List of students in this class (for teachers)
    protected List<UserDTO> StudentsInClass { get; set; } = new();

    // Modal reference and selected profile display data
    private ProfileModal? profileModalRef;
    protected string? SelectedUserName { get; set; }
    protected string? SelectedProfileImageUrl { get; set; }
    protected string? SelectedUserInterests { get; set; }
    protected string? SelectedTitle { get; set; }
    public List<AssignmentDTO> AssignmentList { get; set; } = new();

    protected override async Task OnParametersSetAsync()
    {
        // Determine role
        if (!string.IsNullOrWhiteSpace(UserId))
        {
            var user = await UsersService.GetUserByIdAsync(UserId);
            isStudent = !string.Equals(user?.Role, "Teacher", StringComparison.OrdinalIgnoreCase);
        }

        // If teacher and we have a class id, load students
        if (!isStudent && !string.IsNullOrWhiteSpace(ClassId))
        {
            StudentsInClass = await EnrollmentService.GetStudentsForClassAsync(ClassId) ?? new();
        }

        AssignmentList = await AssignmentService.GetAssignmentsAsync(UserId ?? string.Empty, ClassId ?? string.Empty);
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
