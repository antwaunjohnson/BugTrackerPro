using BugTrackerPro.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace BugTrackerPro.Services.Interfaces;

public interface IBTProRolesService
{
    Task<bool> GetUserInRoleAsync(BTProUser user, string roleName);

    Task<List<IdentityRole>> GetRolesAsync();

    Task<IEnumerable<string>> GetUserRolesAsync(BTProUser user);

    Task<bool> AddUserToRoleAsync(BTProUser user, string roleName);

    Task<bool> RemoveUserFromRoleAsync(BTProUser user, string roleName);    

    Task<bool> RemoveUserFromRolesAsync(BTProUser user, IEnumerable<string> roles);

    Task<List<BTProUser>> GetUsersInRoleAsync(string roleName, int companyId);

    Task<List<BTProUser>> GetUsersNotInRoleAsync(string roleName, int companyId);

    Task<string> GetRoleNameByIdAsync(string roleId);
}
