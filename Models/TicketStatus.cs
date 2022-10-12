using System.ComponentModel;

namespace BugTrackerPro.Models;

public class TicketStatus
{
    public int Id { get; set; }

    [DisplayName("Status Name")]
    public string? Name { get; set; }
}
