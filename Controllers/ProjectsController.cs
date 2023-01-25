using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTrackerPro.Data;
using BugTrackerPro.Models;
using BugTrackerPro.Services.Interfaces;
using BugTrackerPro.Models.ViewModels;
using BugTrackerPro.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using BugTrackerPro.Extensions;

namespace BugTrackerPro.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly IBTProCompanyInfoService _infoService;
        private readonly IBTProRolesService _rolesService;
        private readonly IBTProLookupService _lookupService;
        private readonly IBTProFileService _fileService;
        private readonly IBTProProjectService _projectService;
        private readonly UserManager<BTProUser> _userManager;

        public ProjectsController(IBTProCompanyInfoService infoService, IBTProRolesService rolesService, IBTProLookupService lookupService, IBTProFileService fileService, IBTProProjectService projectService, UserManager<BTProUser> userManager)
        {
            _infoService = infoService;
            _rolesService = rolesService;
            _lookupService = lookupService;
            _fileService = fileService;
            _projectService = projectService;
            _userManager = userManager;
        }

        
        
        // GET: My Projects
        public async Task<IActionResult> MyProjects()
        {
            string userId = _userManager.GetUserId(User);

            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

            return View(projects);
        }

        // GET:All Projects
        public async Task<IActionResult> AllProjects()
        {
            List<Project> projects = new();

            int companyId = User.Identity!.GetCompanyId()!.Value;

            if(User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.ProjectManager)))
            {
                projects = await _infoService.GetAllProjectsAsync(companyId);
            }
            else
            {
                projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);
            }

            return View(projects);
        }

        // GET: Archived Projects
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity!.GetCompanyId()!.Value;

            List<Project> projects = await _projectService.GetArchivedProjectsByCompanyAsync(companyId);

            return View(projects);
        }

        [Authorize(Roles="Admin")]
        // GET: Unassigned Projects
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity!.GetCompanyId()!.Value;

            List<Project> projects = new();

            projects = await _projectService.GetUnassignedProjectsAsync(companyId);

            return View(projects);
        }

        [Authorize(Roles = "Admin")]
        // GET: AssignPM
        public async Task<IActionResult> AssignPM(int projectId)
        {
            int companyId = User.Identity!.GetCompanyId()!.Value;

            AssignPMViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(projectId, companyId);
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(Roles.ProjectManager), companyId), "Id", "FullName");

            return View(model);

        }

        [Authorize(Roles = "Admin")]
        // POST: AssignPM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PMID))
            {
                await _projectService.AddProjectManagerAsync(model.PMID, model.Project!.Id);

                return RedirectToAction(nameof(Details), new { id = model.Project!.Id });
            }
            return RedirectToAction(nameof(AssignPM), new {id = model.Project!.Id});
        }

     

        [Authorize(Roles = "Admin, ProjectManager")]
        // GET: Assign Members
        [HttpGet]
        public async Task<IActionResult> AssignMembers(int id)
        {
            ProjectMembersViewModel model = new();

            int companyId = User.Identity!.GetCompanyId()!.Value;
            model.Project = await _projectService.GetProjectByIdAsync(id, companyId);

            List<BTProUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(Roles.Developer), companyId);
            List<BTProUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(Roles.Submitter), companyId);

            List<BTProUser> companyMembers = developers.Concat(submitters).ToList();

            List<string> projectMembers = model.Project.Members!.Select(m => m.Id).ToList();

            model.Users = new MultiSelectList(companyMembers, "Id", "FullName", projectMembers);

            return View(model);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        // POST: Assign Members
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel model)
        {
            if(model.SelectedUsers != null)
            {
                List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project!.Id)).Select(m => m.Id).ToList();

                foreach(string member in memberIds)
                {
                    await _projectService.RemoveUserFromProjectAsync(member, model.Project.Id);
                }

                foreach(string member in model.SelectedUsers)
                {
                    await _projectService.AddUserToProjectAsync(member, model.Project.Id);
                }

                return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
            }

            return RedirectToAction(nameof(AssignMembers), new { id = model.Project!.Id });
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId()!.Value;

            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);


            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId()!.Value;

            AddProjectWithPMViewModel model = new();

            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");

            
            return View(model);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        // POST: Projects/Create
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( AddProjectWithPMViewModel model)
        {
           if(model != null)
            {
                int companyId = User.Identity!.GetCompanyId()!.Value;

                try
                {
                    if(model.Project?.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.Name;
                        model.Project.ImageFileContentType = model.Project.ImageFormFile.ContentType;
                    }

                    model.Project!.CompanyId = companyId;

                    await _projectService.AddNewProjectAsync(model.Project);

                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PmId, model.Project.Id);
                    }

                }
                catch (Exception)
                {

                    throw;
                }
                //TODO: Redirect to all Projects
                return RedirectToAction("Index");
            }         
            
            return RedirectToAction("Create");
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            int companyId = User.Identity!.GetCompanyId()!.Value;

            AddProjectWithPMViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(id!.Value, companyId);

            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");


            return View(model);
        }

        // POST: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {
            if (model != null)
            {
                try
                {
                    if (model.Project?.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.Name;
                        model.Project.ImageFileContentType = model.Project.ImageFormFile.ContentType;
                    }

                  await _projectService.UpdateProjectAsync(model.Project!);

                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PmId, model.Project!.Id);
                    }

                }
                catch (DbUpdateConcurrencyException)
                {

                    if (!await ProjectExists(model.Project!.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //TODO: Redirect to all Projects
                return RedirectToAction("Index");
            }

            return RedirectToAction("Edit");

        }

        [Authorize(Roles = "Admin, ProjectManager")]
        // GET: Projects/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId()!.Value;

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Archive/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            int companyId = User.Identity!.GetCompanyId()!.Value;
            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            await _projectService.ArchiveProjectAsync(project);
           
            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId()!.Value;

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            int companyId = User.Identity!.GetCompanyId()!.Value;
            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            await _projectService.RestoreProjectAsync(project);

            return RedirectToAction(nameof(Index));
        }
        private async Task<bool> ProjectExists(int id)
        {
            int companyId = User.Identity!.GetCompanyId()!.Value;

            return (await _projectService.GetAllProjectsByCompanyAsync(companyId)).Any(p => p.Id == id);
        }
    }
}
