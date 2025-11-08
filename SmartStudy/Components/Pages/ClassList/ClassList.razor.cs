using Microsoft.AspNetCore.Components;

namespace SmartStudy.Components.Pages.ClassList;

public class MockClass
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string Color { get; set; } = "#4CAF50";
}

public partial class ClassList : ComponentBase
{
    [Parameter]
    public string? UserId { get; set; }

    private readonly List<MockClass> _mockClasses = new()
    {
        new() { Id = "MATH101", Name = "Algebra I", Teacher = "Dr. Smith", Color = "#4CAF50" },
        new() { Id = "BIO202", Name = "Biology II", Teacher = "Prof. Johnson", Color = "#2196F3" },
        new() { Id = "CHEM101", Name = "Chemistry I", Teacher = "Dr. Lee", Color = "#9C27B0" },
        new() { Id = "HIST300", Name = "World History", Teacher = "Prof. Williams", Color = "#FF9800" },
        new() { Id = "PHYS210", Name = "Physics II", Teacher = "Dr. Brown", Color = "#E91E63" }
    };

    [Inject]
    private NavigationManager Nav { get; set; } = default!;

    private void NavigateToClass(string classId)
    {
        if (string.IsNullOrWhiteSpace(UserId)) return;
        Nav.NavigateTo($"/{UserId}/Class/{classId}");
    }
}