using Microsoft.AspNetCore.Components;
using SmartStudy.API.Services;

namespace SmartStudy.Components.Pages.ClassList;

public partial class ClassList : ComponentBase
{
    [Parameter]
    public string? UserId { get; set; }

    private readonly List<ClassCardModel> _classes = new();
    private bool _isLoading;
    private string? _error;

    private static readonly string[] ColorPalette =
    [
        "#4CAF50",
        "#2196F3",
        "#9C27B0",
        "#FF9800",
        "#E91E63",
        "#FF5722",
        "#00BCD4"
    ];

    [Inject]
    private NavigationManager Nav { get; set; } = default!;

    [Inject]
    private ClassesService ClassesService { get; set; } = default!;

    [Inject]
    private UsersService UsersService { get; set; } = default!;

    protected override async Task OnParametersSetAsync()
    {
        _error = null;
        _classes.Clear();

        if (string.IsNullOrWhiteSpace(UserId))
        {
            _error = "Missing user id.";
            return;
        }

        _isLoading = true;

        try
        {
            var classDtos = await ClassesService.GetClassesForUserAsync(UserId);

            if (classDtos.Count == 0)
            {
                var user = await UsersService.GetUserByIdAsync(UserId);
                if (user is null)
                {
                    _error = "User not found.";
                }
                return;
            }

            var teacherIds = classDtos
                .Select(c => c.TeacherId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var teacherLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (teacherIds.Count > 0)
            {
                var teacherTasks = teacherIds.Select(id => UsersService.GetUserByIdAsync(id));
                var teachers = await Task.WhenAll(teacherTasks);

                foreach (var teacher in teachers)
                {
                    if (teacher is not null)
                    {
                        teacherLookup[teacher.Id] = teacher.DisplayName;
                    }
                }
            }

            var index = 0;
            foreach (var schoolClass in classDtos)
            {
                var teacherName = teacherLookup.TryGetValue(schoolClass.TeacherId, out var displayName)
                    ? displayName
                    : string.IsNullOrWhiteSpace(schoolClass.TeacherId)
                        ? "Instructor"
                        : schoolClass.TeacherId;

                _classes.Add(new ClassCardModel
                {
                    Id = schoolClass.Id,
                    Name = schoolClass.Name,
                    Teacher = teacherName,
                    Color = ColorPalette[index % ColorPalette.Length],
                    ImageUrl = schoolClass.ImageUrl
                });

                index++;
            }
        }
        catch
        {
            _error = "Unable to load classes right now.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void NavigateToClass(string classId)
    {
        if (string.IsNullOrWhiteSpace(UserId)) return;
        Nav.NavigateTo($"/{UserId}/Class/{classId}");
    }

    private sealed class ClassCardModel
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Teacher { get; init; } = string.Empty;
        public string Color { get; init; } = "#4CAF50";
        public string ImageUrl { get; init; } = string.Empty;
    }
}
