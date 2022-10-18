using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTrackerPro.Models.ViewModels;

public class ManageUserRolesViewModel
{
    public BTProUser? BTProUser { get; set; }

    public MultiSelectList? Roles { get; set; }

    public List<string>? SelectedRoles { get; set; }
}
