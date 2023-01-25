using BugTrackerPro.Extensions;
using BugTrackerPro.Models;
using BugTrackerPro.Models.ViewModels;
using BugTrackerPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTrackerPro.Controllers;

[Authorize]
public class UserRolesController : Controller
{

    private readonly IBTProRolesService _rolesService;
    private readonly IBTProCompanyInfoService _companyInfoService;
    public UserRolesController(IBTProRolesService rolesService, IBTProCompanyInfoService companyInfoService)
    {
        _rolesService = rolesService;
        _companyInfoService = companyInfoService;
    }

    [HttpGet]
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
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
    {
        int companyId = User.Identity!.GetCompanyId()!.Value;

        BTProUser? btpUser = (await _companyInfoService.GetAllMembersAsync(companyId)).FirstOrDefault(u => u.Id == member.BTProUser!.Id);

        IEnumerable<string> roles = await _rolesService.GetUserRolesAsync(btpUser!);


        string? userRole = member.SelectedRoles!.FirstOrDefault();

        if (!string.IsNullOrEmpty(userRole))
        {
            if(await _rolesService.RemoveUserFromRolesAsync(btpUser!, roles))
            {
                await _rolesService.AddUserToRoleAsync(btpUser!, userRole);
            }
        }

        return RedirectToAction(nameof(ManageUserRoles));
    }
}
