using BugTrackerPro.Data;
using BugTrackerPro.Models;
using BugTrackerPro.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BugTrackerPro.Services;

public class BTProRolesService : IBTProRolesService
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<BTProUser> _userManager;

    public BTProRolesService(ApplicationDbContext context,
                                RoleManager<IdentityRole> roleManager,                      UserManager<BTProUser> userManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<bool> AddUserToRoleAsync(BTProUser user, string roleName)
    {
        bool result = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
        return result;
    }

    public async Task<string> GetRoleNameByIdAsync(string roleId)
    {
        IdentityRole? role = _context.Roles.Find(roleId);
        string result = await _roleManager.GetRoleNameAsync(role!);
        return result;
    }

    public async Task<bool> GetUserInRoleAsync(BTProUser user, string roleName)
    {
        bool result = await _userManager.IsInRoleAsync(user, roleName);
        return result;
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(BTProUser user)
    {
        IEnumerable<string>? result = await _userManager.GetRolesAsync(user);
        return result;
    }

    public async Task<List<BTProUser>> GetUsersInRoleAsync(string roleName, int companyId)
    {
        List<BTProUser> users = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
        List<BTProUser> result = users.Where(u => u.CompanyId == companyId).ToList();
        return result;
    }

    public async Task<List<BTProUser>> GetUsersNotInRoleAsync(string roleName, int companyId)
    {
        List<string> userIds = (await _userManager.GetUsersInRoleAsync(roleName)).Select(u => u.Id).ToList();
        List<BTProUser> roleUsers = _context.Users.Where(u => !userIds.Contains(u.Id)).ToList();
        List<BTProUser> result = roleUsers.Where(u => u.CompanyId == companyId).ToList();
        return result;
    }

    public async Task<bool> RemoveUserFromRoleAsync(BTProUser user, string roleName)
    {
        bool result = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
        return result;
    }

    public async Task<bool> RemoveUserFromRolesAsync(BTProUser user, IEnumerable<string> roles)
    {
        bool result = (await _userManager.RemoveFromRolesAsync(user, roles)).Succeeded;
        return result;
    }
}
