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
using Microsoft.AspNetCore.Identity;
using BugTrackerPro.Extentions;
using BugTrackerPro.Models.Enums;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.AspNetCore.Authorization;
using BugTrackerPro.Models.ViewModels;

namespace BugTrackerPro.Controllers
{
    public class TicketsController : Controller
    {
        private readonly UserManager<BTProUser> _userManager;
        private readonly IBTProLookupService _lookupService;
        private readonly IBTProProjectService _projectService;
        private readonly IBTProTicketService _ticketService;
        private readonly IBTProFileService _fileService;
        private readonly IBTProTicketHistoryService _historyService;

        public TicketsController(UserManager<BTProUser> userManager, IBTProProjectService projectService, IBTProLookupService lookupService, IBTProTicketService ticketService, IBTProFileService fileService, IBTProTicketHistoryService historyService)
        {
            
            _userManager = userManager;
            _projectService = projectService;
            _lookupService = lookupService;
            _ticketService = ticketService;
            _fileService = fileService;
            _historyService = historyService;
        }

        
      

        //GET: MyTickets
        public async Task<IActionResult> MyTickets()
        {
            BTProUser btpUser = await _userManager.GetUserAsync(User);

            List<Ticket>? tickets = await _ticketService.GetTicketsByUserIdAsync(btpUser.Id, btpUser.CompanyId);

            return View(tickets);
        }

        //GET: AllTickets
        public async Task<IActionResult> AllTickets()
        {
            int companyId = User.Identity!.GetCompanyId()!.Value;

            List<Ticket>? tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);

            if(User.IsInRole(nameof(Roles.Developer)) || User.IsInRole(nameof(Roles.Submitter)))
            {
                return View(tickets.Where(t => t.Archived == false));
            }
            else
            {
                return View(tickets);
            }
            
        }

        //GET: ArchivedTickets
        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = User.Identity!.GetCompanyId()!.Value;

            List<Ticket> tickets = await _ticketService.GetArchivedTicketsAsync(companyId);

            return View(tickets);
        }

        //GET: UnassignedTickets
        [Authorize(Roles="Admin,ProjectManager")]
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity!.GetCompanyId()!.Value;
            string btpUserId = _userManager.GetUserId(User);


            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId);

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                return View(tickets);
            }
            else
            {
                List<Ticket> pmTickets = new();

                foreach(Ticket ticket in tickets)
                {
                    if(await _projectService.IsAssignedProjectManagerAsync(btpUserId, ticket.ProjectId))
                    {
                        pmTickets.Add(ticket);
                    }
                }

                return View(pmTickets);
            }

        }

        [HttpGet]
        public async Task<IActionResult> AssignDeveloper(int id)
        {
            AssignDeveloperViewModel model = new();

            model.Ticket = await _ticketService.GetTicketByIdAsync(id);
            model.Developers = new SelectList(await _projectService.GetProjectMembersByRoleAsync(model.Ticket.ProjectId, nameof(Roles.Developer)), "Id", "FullName");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel model)
        {
            if(model.DeveloperId != null)
            {
                BTProUser btpUser = await _userManager.GetUserAsync(User);

                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket!.Id);

                try
                {
                    await _ticketService.AssignTicketAsync(model.Ticket!.Id, model.DeveloperId);
                }
                catch (Exception)
                {

                    throw;
                }

                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket!.Id);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, btpUser.Id);

                return RedirectToAction(nameof(Details), new { id = model.Ticket!.Id });
            }

            


            return RedirectToAction(nameof(AssignDeveloper), new { id = model.Ticket!.Id });
        }


        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            } 

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            BTProUser btpUser = await _userManager.GetUserAsync(User);

            int companyId = User.Identity!.GetCompanyId()!.Value; 

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(btpUser.Id), "Id", "Name");
            }
           
           
            
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(),"Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId,")] Ticket ticket)
        {
            BTProUser btpUser = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {

                try
                {
                    ticket.Created = DateTimeOffset.Now;
                    ticket.OwnerUserId = btpUser.Id;

                    ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync(nameof(BTProTicketStatus.New))).Value;

                    await _ticketService.AddNewTicketAsync(ticket);

                    //TODO: Ticket History
                    Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                    await _historyService.AddHistoryAsync(null!, newTicket, btpUser.Id);

                    //TODO: Ticket Notification

                }
                catch (Exception)
                {

                    throw;
                }




                return RedirectToAction(nameof(Index));
            }

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(btpUser.CompanyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(btpUser.Id), "Id", "Name");
            }



            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }
           
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                BTProUser btpUser = await _userManager.GetUserAsync(User);
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);

                try
                {
                    ticket.Updated = DateTimeOffset.Now;
                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                //TODO: Add Ticket History
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, btpUser.Id);

                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id,TicketId,Comment")] TicketComment ticketComment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ticketComment.UserId = _userManager.GetUserId(User);
                    ticketComment.Created = DateTimeOffset.Now;

                    await _ticketService.AddTicketCommentAsync(ticketComment);

                    await _historyService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId);
                }
                catch (Exception)
                {

                    throw;
                }
            }
            return RedirectToAction("Details", new { id = ticketComment.TicketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if(ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                try
                {
                    ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                    ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                    ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;

                    ticketAttachment.Created = DateTimeOffset.Now;
                    ticketAttachment.UserId = _userManager.GetUserId(User);

                    await _ticketService.AddTicketAttachmentAsync(ticketAttachment);

                    await _historyService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.UserId);

                }
                catch (Exception)
                {

                    throw;
                }
                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";
            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message=statusMessage });
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName!;
            byte[] fileData = ticketAttachment.FileData!;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        // GET: Tickets/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            ticket.Archived = true;
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction(nameof(Index));
        }

        // GET: Tickets/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            ticket.Archived = false;
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> TicketExists(int id)
        {
            int companyId = User.Identity!.GetCompanyId()!.Value;

            return (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).Any(t => t.Id == id);
        }
    }
}
