using BugTrackerPro.Data;
using BugTrackerPro.Models;
using BugTrackerPro.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerPro.Services;

public class BTProRolesService : IBTProRolesService
{
   
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<BTProUser> _userManager;

  

    
    public BTProRolesService(ApplicationDbContext context,
                             RoleManager<IdentityRole> roleManager, UserManager<BTProUser> userManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }
   

    #region Add User To Role
    public async Task<bool> AddUserToRoleAsync(BTProUser user, string roleName)
    {
        bool result = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
        return result;
    }
    #endregion

    #region Get Role Name By Id
    public async Task<string> GetRoleNameByIdAsync(string roleId)
    {
        IdentityRole? role = _context.Roles.Find(roleId);
        string result = await _roleManager.GetRoleNameAsync(role!);
        return result;
    }
    #endregion

    #region Get Roles
    public async Task<List<IdentityRole>> GetRolesAsync()
    {
        try
        {
            List<IdentityRole> result = new();

            result = await _context.Roles.ToListAsync();

            return result;
        }
        catch (Exception)
        {

            throw;
        }
    }
    #endregion

    #region Get User In Role
    public async Task<bool> GetUserInRoleAsync(BTProUser user, string roleName)
    {
        bool result = await _userManager.IsInRoleAsync(user, roleName);
        return result;
    }
    #endregion

    #region Get User Roles
    public async Task<IEnumerable<string>> GetUserRolesAsync(BTProUser user)
    {
        IEnumerable<string>? result = await _userManager.GetRolesAsync(user);
        return result;
    }
    #endregion

    #region Get Users In Roles
    public async Task<List<BTProUser>> GetUsersInRoleAsync(string roleName, int companyId)
    {
        try
        {
            List<BTProUser> users = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
            List<BTProUser> result = users.Where(u => u.CompanyId == companyId).ToList();
            return result;
        }
        catch (Exception)
        {

            throw;
        }
    }
    #endregion

    #region Get Users Not In Role
    public async Task<List<BTProUser>> GetUsersNotInRoleAsync(string roleName, int companyId)
    {
        List<string> userIds = (await _userManager.GetUsersInRoleAsync(roleName)).Select(u => u.Id).ToList();
        List<BTProUser> roleUsers = _context.Users.Where(u => !userIds.Contains(u.Id)).ToList();
        List<BTProUser> result = roleUsers.Where(u => u.CompanyId == companyId).ToList();
        return result;
    }
    #endregion

    #region Remove User From Role
    public async Task<bool> RemoveUserFromRoleAsync(BTProUser user, string roleName)
    {
        bool result = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
        return result;
    }
    #endregion

    #region Remove User From Roles
    public async Task<bool> RemoveUserFromRolesAsync(BTProUser user, IEnumerable<string> roles)
    {
        bool result = (await _userManager.RemoveFromRolesAsync(user, roles)).Succeeded;
        return result;
    } 
    #endregion
}
