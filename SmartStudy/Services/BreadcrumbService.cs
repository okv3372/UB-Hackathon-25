using Microsoft.AspNetCore.Components;

namespace SmartStudy.Services;

public class BreadcrumbItem
{
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}

public class BreadcrumbService
{
    private readonly NavigationManager _nav;

    public BreadcrumbService(NavigationManager nav) => _nav = nav;

    // Build a clean, rule-based breadcrumb list from current or provided URI
    public List<BreadcrumbItem> GetBreadcrumbsFromUri(string? location = null)
    {
        var uri = new Uri(location ?? _nav.Uri);
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        var items = new List<BreadcrumbItem>();

        // Always start with Home
        items.Add(new BreadcrumbItem { Label = "Home", Url = "/", IsCurrent = segments.Length == 0 });

        if (segments.Length == 0)
            return items;

        // Expecting routes like:
        // /{userId}/ClassList
        // /{userId}/Class/{classId}
        // /{userId}/Class/{classId}/Student/{studentId}
        // /{userId}/Class/{classId}/Student/{studentId}/assignment/{assignmentId}
        // We never emit a crumb for raw "/Class" (dead route); instead we insert "Class List"
        var userId = segments[0];

        // Helper to add a crumb if it's not identical to the last one
        void AddCrumb(string label, string url, bool isCurrent = false)
        {
            var last = items.LastOrDefault();
            if (last is null || !string.Equals(last.Label, label, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(last.Url, url, StringComparison.OrdinalIgnoreCase))
            {
                items.Add(new BreadcrumbItem { Label = label, Url = url, IsCurrent = isCurrent });
            }
            else if (isCurrent)
            {
                last.IsCurrent = true;
            }
        }

        // Handle second segment (if any)
        if (segments.Length >= 2)
        {
            var second = segments[1].ToLowerInvariant();

            if (second == "classlist")
            {
                // /{userId}/ClassList
                var listUrl = $"/{userId}/ClassList";
                AddCrumb("Class List", listUrl, isCurrent: segments.Length == 2);
                return MarkTailAsCurrent(items);
            }

            if (second == "class")
            {
                // Always link back to Class List (avoid "/{userId}/Class" broken route)
                var listUrl = $"/{userId}/ClassList";
                AddCrumb("Class List", listUrl, isCurrent: false);

                // If we have a classId, add it
                if (segments.Length >= 3)
                {
                    var classId = segments[2];
                    var classUrl = $"/{userId}/Class/{classId}";
                    var isTail = segments.Length == 3;
                    AddCrumb(classId, classUrl, isTail);

                    // Student branch
                    if (segments.Length >= 4 && segments[3].Equals("student", StringComparison.OrdinalIgnoreCase))
                    {
                        if (segments.Length >= 5)
                        {
                            var studentId = segments[4];
                            var studentUrl = $"/{userId}/Class/{classId}/Student/{studentId}";
                            var tailHere = segments.Length == 5;
                            // Collapse "Student" + "studentId" into one crumb to avoid duplicates
                            AddCrumb("Student", studentUrl, tailHere);

                            // Assignment branch
                            if (!tailHere && segments.Length >= 6 && segments[5].Equals("assignment", StringComparison.OrdinalIgnoreCase))
                            {
                                // Optional assignmentId; label simply "Assignment"
                                var assignmentUrl = segments.Length >= 7
                                    ? $"/{userId}/Class/{classId}/Student/{studentId}/assignment/{segments[6]}"
                                    : $"/{userId}/Class/{classId}/Student/{studentId}/assignment";

                                AddCrumb("Assignment", assignmentUrl, isCurrent: true);
                            }
                        }
                    }
                }

                return MarkTailAsCurrent(items);
            }
        }

        // Default: mark the last as current
        return MarkTailAsCurrent(items);
    }

    private static List<BreadcrumbItem> MarkTailAsCurrent(List<BreadcrumbItem> items)
    {
        for (int i = 0; i < items.Count; i++)
            items[i].IsCurrent = (i == items.Count - 1);
        return items;
    }
}
