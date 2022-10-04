using System.ComponentModel;

namespace BugTrackerPro.Models;

public class ProjectPriority
{
    public int Id { get; set; }

    [DisplayName("Priority Name")]
    public string? Name { get; set; }
}
