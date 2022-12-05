namespace BugTrackerPro.Models.ViewModels;

public class DashboardViewModel
{
    public Company? Company { get; set; }

    public List<Project>? Projects { get; set; }

    public List<Ticket>? Tickets { get; set; }

    public List<BTProUser>? Members { get; set; }
}
