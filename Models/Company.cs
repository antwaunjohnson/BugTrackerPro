using System.ComponentModel;

namespace BugTrackerPro.Models;

public class Company
{
    public int Id { get; set; }

    
    [DisplayName("Company Name")]
    public string? Name { get; set; }

    [DisplayName("Company Description")]
    public string? Description { get; set; }


    public virtual ICollection<BTProUser>? Members { get; set; }

    public virtual ICollection<Project>? Projects { get; set; }

    public virtual ICollection<Invite>? Invites { get; set; }
}
