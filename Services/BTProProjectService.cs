﻿using BugTrackerPro.Data;
using BugTrackerPro.Models;
using BugTrackerPro.Models.Enums;
using BugTrackerPro.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerPro.Services;

public class BTProProjectService : IBTProProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly IBTProRolesService _roleService;

    public BTProProjectService(ApplicationDbContext context,IBTProRolesService roleService)
    {
        _context = context;
        _roleService = roleService;
    }

    public async Task AddNewProjectAsync(Project project)
    {
        _context.Add(project);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
    {
        BTProUser currentPM = await GetProjectManagerAsync(projectId);

        if (currentPM != null)
        {
            try
            {
                await RemoveProjectManagerAsync(projectId);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error removing current Pm. - Error: {ex.Message}");
                return false;
            }
        }

        try
        {
            await AddUserToProjectAsync(userId, projectId);
            return true;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error adding new Pm. - Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
    {
        BTProUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if(user != null)
        {
            Project? project = await _context.Projects!.FirstOrDefaultAsync(p => p.Id == projectId);
            if(!await IsUserOnProjectAsync(userId, projectId))
            {
                try
                {
                    project?.Members?.Add(user);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch(Exception)
                {
                    throw;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public async Task ArchiveProjectAsync(Project project)
    {
        try
        {
            project.Archived = true;
            await UpdateProjectAsync(project);

            foreach (Ticket ticket in project.Tickets!)
            {
                ticket.ArchivedByProject = true;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception)
        {

            throw;
        }

    }

    public async Task<List<BTProUser>> GetAllProjectMembersExceptPMAsync(int projectId)
    {
        List<BTProUser> developers = await GetProjectMembersByRoleAsync(projectId, Roles.Developer.ToString());
        List<BTProUser> submitters = await GetProjectMembersByRoleAsync(projectId, Roles.Submitter.ToString());
        List<BTProUser> admins = await GetProjectMembersByRoleAsync(projectId, Roles.Admin.ToString());

        List<BTProUser> teamMembers = developers.Concat(submitters).Concat(admins).ToList();

        return teamMembers;
    }

    public async Task<List<Project>> GetAllProjectsByCompany(int companyId)
    {
        List<Project> projects = new();
        projects = await _context.Projects!.Where(p => p.CompanyId == companyId && p.Archived == false)
                                .Include(p => p.Members!)
                                .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.Comments!)
                                .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.Attachments!)
                                .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.History!)
                                .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.Notifications!)
                                .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.DeveloperUser!)
                                .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.OwnerUser!)
                                 .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.TicketStatus!)
                                 .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.TicketPriority!)
                                 .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.TicketType)
                                .Include(p => p.ProjectPriority)
                                .ToListAsync();
        return projects;
    }

    public async Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName)
    {
        List<Project> projects = await GetAllProjectsByCompany(companyId);
        int priorityId = await LookProjectPriorityIdAsync(priorityName);

        return projects.Where(p => p.ProjectPriorityId == priorityId).ToList();
    }

    public async Task<List<Project>> GetArchivedProjectsByCompany(int companyId)
    {
        List<Project> projects =  await _context.Projects!.Where(p => p.CompanyId == companyId && p.Archived == true)
                                .Include(p => p.Members!)
                                .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.Comments!)
                                .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.Attachments!)
                                .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.History!)
                                .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.Notifications!)
                                .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.DeveloperUser!)
                                .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.OwnerUser!)
                                 .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.TicketStatus!)
                                 .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.TicketPriority!)
                                 .Include(p => p.Tickets!)
                                    .ThenInclude(t => t.TicketType)
                                .Include(p => p.ProjectPriority)
                                .ToListAsync();

        return projects;
    }

    public Task<List<BTProUser>> GetDevelopersOnProjectAsync(int projectId)
    {
        throw new NotImplementedException();
    }

    public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
    {
        Project? project = await _context.Projects!
            .Include(p => p.Tickets!)
            .Include(p => p.Members!)
            .Include(p => p.ProjectPriority!)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

        return project!;
    }

    public async Task<BTProUser> GetProjectManagerAsync(int projectId)
    {
        Project? project = await _context.Projects!
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        foreach(BTProUser member in project?.Members!)
        {
            if (await _roleService.GetUserInRoleAsync(member, Roles.ProjectManager.ToString()))
            {
                return member;
            }
        }
        return null!;
    }

    public async Task<List<BTProUser>> GetProjectMembersByRoleAsync(int projectId, string role)
    {
        Project? project = await _context.Projects!
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        List<BTProUser> members = new();

        foreach (var user in project?.Members!)
        {
            if(await _roleService.GetUserInRoleAsync(user, role))
            {
                members.Add(user);
            }
        }

        return members;
    }

    public Task<List<BTProUser>> GetSubmittersOnProjectAsync(int projectId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Project>> GetUserProjectsAsync(string? userId)
    {
        try
        {
            List<Project>? userProjects = (await _context.Users!
                .Include(u => u.Projects!)
                    .ThenInclude(p => p.Company!)
                .Include(u => u.Projects!)
                    .ThenInclude(p => p.Members!)
                .Include(u => u.Projects!)
                    .ThenInclude(p => p.Tickets!)
                .Include(u => u.Projects!)
                    .ThenInclude(t => t.Tickets!)
                        .ThenInclude(t => t.DeveloperUser!)
                .Include(u => u.Projects!)
                    .ThenInclude(t => t.Tickets!)
                        .ThenInclude(t => t.OwnerUser!)
                .Include(u => u.Projects!)
                    .ThenInclude(t => t.Tickets!)
                        .ThenInclude(t => t.TicketPriority!)
                .Include(u => u.Projects!)
                    .ThenInclude(t => t.Tickets!)
                        .ThenInclude(t => t.TicketStatus!)
                .Include(u => u.Projects!)
                    .ThenInclude(t => t.Tickets!)
                        .ThenInclude(t => t.TicketType!)
                .FirstOrDefaultAsync(u => u.Id == userId))!.Projects!.ToList();

            return userProjects;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"**** ERROR **** - Error Getting user projects list. ---> {ex.Message}");
            throw;
        }
    }

    public async Task<List<BTProUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
    {
        List<BTProUser> users = await _context.Users.Where(u => u.Projects!.All(p => p.Id != projectId)).ToListAsync();

        return users.Where(u => u.CompanyId == companyId).ToList();
    }

    public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
    {
        Project? project = await _context.Projects!
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        bool result = false;

        if(project != null)
        {
            result = project.Members!.Any(m => m.Id == userId);
        }
        return result;
    }

    public async Task<int> LookProjectPriorityIdAsync(string priorityName)
    {
        int priorityId = (await _context.ProjectPriorities!.FirstOrDefaultAsync(p => p.Name == priorityName))!.Id;
        return priorityId;
    }

    public async Task RemoveProjectManagerAsync(int projectId)
    {
        Project? project = await _context.Projects!
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        try
        {
            foreach(BTProUser member in project?.Members!)
            {
                if(await _roleService.GetUserInRoleAsync(member, Roles.ProjectManager.ToString()))
                {
                    await RemoveUserFromProjectAsync(member.Id, projectId);
                }
            }
        }
        catch
        {
            throw;
        }
    }

    public async Task RemoveUserFromProjectAsync(string userId, int projectId)
    {
        try
        {
            BTProUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            Project? project = await _context.Projects!.FirstOrDefaultAsync(p => p.Id == projectId);
            
            try
            {
                if (await IsUserOnProjectAsync(userId, projectId))
                {
                    project?.Members?.Remove(user!);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"**** ERROR **** - Error Removing User from project. ---> {ex.Message}");
        }
    }

    public async Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
    {
        try
        {
            List<BTProUser> members = await GetProjectMembersByRoleAsync(projectId, role);
            Project? project = await _context.Projects!.FirstOrDefaultAsync(p => p.Id == projectId);

            foreach(BTProUser btpUser in members)
            {
                try
                {
                    project?.Members!.Remove(btpUser);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"**** ERROR **** - Error Removing Users from project. ---> {ex.Message}");
            throw;
        }
    }

    public async Task RestoreProjectAsync(Project project)
    {
        try
        {
            project.Archived = false;
            await UpdateProjectAsync(project);

            foreach (Ticket ticket in project.Tickets!)
            {
                ticket.ArchivedByProject = false;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task UpdateProjectAsync(Project project)
    {
        _context.Update(project);
        await _context.SaveChangesAsync();
    }
}
