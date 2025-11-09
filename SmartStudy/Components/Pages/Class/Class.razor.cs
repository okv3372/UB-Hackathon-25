using Microsoft.AspNetCore.Components;
using SmartStudy.API.Services;
using SmartStudy.Services;
using SmartStudy.Models;

namespace SmartStudy.Components.Pages.Class;

public partial class Class : ComponentBase
{
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Inject] public UsersService UsersService { get; set; } = default!;
    [Inject] public EnrollmentService EnrollmentService { get; set; } = default!;
    [Inject] public AssignmentService AssignmentService { get; set; } = default!;
    [Inject] public ClassesService ClassesService { get; set; } = default!;

    [Parameter] public string? UserId { get; set; }
    [Parameter] public string? ClassId { get; set; }

    protected bool isStudent { get; set; }

    // List of students in this class (for teachers)
    protected List<UserDTO> StudentsInClass { get; set; } = new();

    // Modal references
    private ProfileModal? profileModalRef;
    private SmartStudy.Components.Shared.UploadAssignmentModal? uploadModalRef;

    // Upload modal state
    protected string? UploadTargetUserId { get; set; }
    protected string? UploadStudentName { get; set; }
    protected string? UploadProfileImageUrl { get; set; }

    private readonly Dictionary<string, ProfileDTO> _profiles = new();

    public List<AssignmentDTO> AssignmentList { get; set; } = new();
    protected ClassDTO? CurrentClass { get; set; }

    // Base path where files are served from (ensure app.UseStaticFiles(); and files are under wwwroot/uploads)
    protected string UploadBasePath => "/uploads";

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

        // Load current class meta for displaying class name
        if (!string.IsNullOrWhiteSpace(ClassId))
        {
            CurrentClass = await ClassesService.GetClassByIdAsync(ClassId);
        }

        // Load assignments for this user & class
        AssignmentList = await AssignmentService.GetAssignmentsAsync(UserId ?? string.Empty, ClassId ?? string.Empty);
    }

    protected void OpenProfileModalForStudent(ProfileDTO student)
    {
        profileModalRef?.OpenForUser(student);
    }

    protected void ShowUploadModalForStudent(UserDTO student)
    {
        UploadTargetUserId = student.Id;
        UploadStudentName = student.DisplayName ?? student.Username ?? "Unknown";
        uploadModalRef?.OpenModal();
    }

    protected void OpenAssignmentsPageForStudent(UserDTO student)
    {
        if (string.IsNullOrWhiteSpace(student.Id) || string.IsNullOrWhiteSpace(ClassId)) return;
        var current = string.IsNullOrWhiteSpace(UserId) ? "me" : UserId;
        NavigationManager.NavigateTo($"/{current}/Class/{ClassId}/student/{student.Id}");
    }

    protected Task OnAssignmentUploaded(AssignmentDTO dto)
    {
        Console.WriteLine($"[Class] Assignment uploaded: Id={dto.Id}, Title={dto.Title}, File={dto.FileName}, Path={dto.FilePath}");
        return Task.CompletedTask;
    }
    
}
