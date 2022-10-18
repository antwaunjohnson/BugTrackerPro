using BugTrackerPro.Extentions;
using BugTrackerPro.Models;
using BugTrackerPro.Models.ViewModels;
using BugTrackerPro.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTrackerPro.Controllers;
public class UserRolesController : Controller
{
    private readonly IBTProRolesService _rolesService;
    private readonly IBTProCompanyInfoService _companyInfoService;
    public UserRolesController(IBTProRolesService rolesService, IBTProCompanyInfoService companyInfoService)
    {
        _rolesService = rolesService;
        _companyInfoService = companyInfoService;
    }
    public async Task<IActionResult> ManageUserRoles()
    {
        // Add an instance of the veiwModel as a list (model)
        List<ManageUserRolesViewModel> model = new();

        // Get Company Id
        int companyId = User.Identity!.GetCompanyId()!.Value;

        // Get all company users
        List<BTProUser> users = await _companyInfoService.GetAllMembersAsync(companyId);

        //Loop over the users to populate the ViewModel
        // - instantiate ViewModel
        // - use _roleService
        // - Create multi-selects

        foreach(BTProUser user in users)
        {
            ManageUserRolesViewModel viewModel = new();
            viewModel.BTProUser = user;
            IEnumerable<string> selected = await _rolesService.GetUserRolesAsync(user);
            viewModel.Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", selected);

            model.Add(viewModel);
        }

        // Return the model to the View
        return View();
    }
}
