using BugTrackerPro.Models;

namespace BugTrackerPro.Services.Interfaces;

public interface IBTProProjectService
{
    Task AddNewProjectAsync(Project project);

    Task<bool> AddProjectManagerAsync(string userId, int projectId);

    Task<bool> AddUserToProjectAsync(string userId, int projectId);

    Task ArchiveProjectAsync(Project project);

    Task<List<Project>> GetAllProjectsByCompanyAsync(int companyId);

    Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName);

    Task<List<BTProUser>> GetAllProjectMembersExceptPMAsync(int projectId);

    Task<List<Project>> GetArchivedProjectsByCompany(int companyId);

    Task<List<BTProUser>>GetDevelopersOnProjectAsync(int projectId);

    Task<BTProUser> GetProjectManagerAsync(int projectId);

    Task<List<BTProUser>> GetProjectMembersByRoleAsync(int projectId, string role);

    Task<Project> GetProjectByIdAsync(int projectId, int companyId);

    Task<List<BTProUser>> GetSubmittersOnProjectAsync(int projectId);

    Task<List<BTProUser>> GetUsersNotOnProjectAsync(int projectId, int companyId);

    Task<List<Project>> GetUserProjectsAsync(string userId);

    Task<bool> IsAssignedProjectManagerAsync(string userId, int projectId);

    Task<bool> IsUserOnProjectAsync(string userId, int projectId);

    Task<int> LookProjectPriorityIdAsync(string priorityName);

    Task RemoveProjectManagerAsync(int projectId);
    
    Task RemoveUsersFromProjectByRoleAsync(string role, int projectId);

    Task RemoveUserFromProjectAsync(string userId, int projectId);

    Task RestoreProjectAsync(Project project);

    Task UpdateProjectAsync(Project project);
    
    


}
