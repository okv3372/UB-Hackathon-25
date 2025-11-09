using Microsoft.AspNetCore.Components;
using SmartStudy.API.Services;
using SmartStudy.Services;
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

    private readonly Dictionary<string, ProfileDTO> _profiles = new();
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
        _profiles.Clear();
        foreach (var student in StudentsInClass)
        {
            var profile = ProfileService.GetProfile(student.Id);
            if (profile is not null)
            {
                _profiles[student.Id] = profile;
            }
        }

        AssignmentList = await AssignmentService.GetAssignmentsAsync(UserId ?? string.Empty, ClassId ?? string.Empty);
    }

    private void LoadProfileFromService(UserProfileCard card)
    {
        SelectedUserName = card.UserName;
        SelectedTitle = card.UserTitle;

        SmartStudy.Models.ProfileDTO? profile = null;
        if (!string.IsNullOrWhiteSpace(card.TargetUserId))
        {
            profile = ProfileService.GetProfile(card.TargetUserId);
        }

        SelectedProfileImageUrl = !string.IsNullOrWhiteSpace(profile?.PictureUrl)
            ? profile.PictureUrl
            : (!string.IsNullOrWhiteSpace(card.ProfileImageUrl) ? card.ProfileImageUrl : "/favicon.png");

        SelectedTitle = !string.IsNullOrWhiteSpace(profile?.GradeLevel)
            ? $"Grade {profile.GradeLevel}"
            : card.UserTitle;

        SelectedUserInterests = !string.IsNullOrWhiteSpace(profile?.Bio)
            ? profile.Bio
            : "No profile details available.";
    }

    protected void OnStudentViewProfile(UserProfileCard card)
    {
        LoadProfileFromService(card);
        profileModalRef?.OpenModal();
    }

    private string GetProfileImageUrl(UserDTO student)
    {
        if (_profiles.TryGetValue(student.Id, out var profile) &&
            !string.IsNullOrWhiteSpace(profile.PictureUrl))
        {
            return profile.PictureUrl!;
        }

        var initials = (student.DisplayName ?? student.Username ?? "U")[0].ToString().ToUpperInvariant();
        return $"https://via.placeholder.com/90/F7C59F/004E89?text={initials}";
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
