﻿using BugTrackerPro.Data;
using BugTrackerPro.Models;
using BugTrackerPro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerPro.Services;

public class BTProTicketHistoryService : IBTProTicketHistoryService
{
    private readonly ApplicationDbContext _context;
    public BTProTicketHistoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Add History (1)
    public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
    {
        if (oldTicket == null && newTicket != null)
        {
            TicketHistory history = new()
            {
                TicketId = newTicket.Id,
                Property = "",
                OldValue = "",
                NewValue = "",
                Created = DateTimeOffset.Now,
                UserId = userId,
                Description = "New Ticket Created"
            };

            try
            {
                await _context.TicketHistories!.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        else
        {
            if (oldTicket?.Title != newTicket?.Title)
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket!.Id,
                    Property = "Title",
                    OldValue = oldTicket?.Title,
                    NewValue = newTicket?.Title,
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = $"New ticket title: {newTicket?.Title}"
                };
                await _context.TicketHistories!.AddAsync(history);
            }

            if (oldTicket?.Description != newTicket?.Description)
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket!.Id,
                    Property = "Description",
                    OldValue = oldTicket?.Description,
                    NewValue = newTicket.Description,
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = $"New ticket description: {newTicket.Description}"
                };
                await _context.TicketHistories!.AddAsync(history);
            }

            if (oldTicket?.TicketPriorityId != newTicket?.TicketPriorityId)
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket!.Id,
                    Property = "TicketPriority",
                    OldValue = oldTicket?.TicketPriority?.Name,
                    NewValue = newTicket?.TicketPriority?.Name,
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = $"New ticket priority: {newTicket?.TicketPriority?.Name}"
                };
                await _context.TicketHistories!.AddAsync(history);
            }

            if (oldTicket?.TicketStatusId != newTicket?.TicketStatusId)
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket!.Id,
                    Property = "TicketStatus",
                    OldValue = oldTicket?.TicketStatus?.Name,
                    NewValue = newTicket?.TicketStatus?.Name,
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = $"New ticket status: {newTicket?.TicketStatus?.Name}"
                };
                await _context.TicketHistories!.AddAsync(history);
            }

            if (oldTicket?.TicketTypeId != newTicket?.TicketTypeId)
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket!.Id,
                    Property = "TicketType",
                    OldValue = oldTicket?.TicketType?.Name,
                    NewValue = newTicket?.TicketType?.Name,
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = $"New ticket type: {newTicket?.TicketType?.Name}"
                };
                await _context.TicketHistories!.AddAsync(history);
            }

            if (oldTicket?.DeveloperUserId != newTicket?.DeveloperUserId)
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket!.Id,
                    Property = "Developer",
                    OldValue = oldTicket?.DeveloperUser?.FullName ?? "Not Assigned",
                    NewValue = newTicket?.DeveloperUser?.FullName,
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = $"New ticket developer: {newTicket?.DeveloperUser?.FullName}"
                };
                await _context.TicketHistories!.AddAsync(history);
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
    #endregion

    #region Add History (2)
    public async Task AddHistoryAsync(int ticketId, string model, string userId)
    {
        try
        {
            Ticket? ticket = await _context.Tickets!.FindAsync(ticketId);
            string description = model.ToLower().Replace("ticket", "");
            description = $"New {description} added to ticket: {ticket.Title}";

            TicketHistory? history = new()
            {
                TicketId = ticket.Id,
                Property = model,
                OldValue = "",
                NewValue = "",
                Created = DateTimeOffset.Now,
                UserId = userId,
                Description = description
            };

            await _context.TicketHistories!.AddAsync(history);
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {

            throw;
        }
    }

    #endregion

    #region Get Company Tickets Histories
    public async Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
    {
        try
        {
            List<Project> projects = (await _context.Companies!
                .Include(c => c.Projects!)
                    .ThenInclude(p => p.Tickets!)
                        .ThenInclude(t => t.History!)
                            .ThenInclude(h => h.User!)
                .FirstOrDefaultAsync(c => c.Id == companyId))!.Projects!.ToList();

            List<Ticket> tickets = projects.SelectMany(p => p.Tickets!).ToList();

            List<TicketHistory> ticketHistories = tickets.SelectMany(t => t.History).ToList();

            return ticketHistories;
        }
        catch (Exception)
        {

            throw;
        }
    }
    #endregion

    #region Get Project Tickets Histories
    public async Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId, int companyId)
    {
        try
        {
            Project? project = await _context.Projects!.Where(p => p.CompanyId == companyId)
                .Include(p => p.Tickets!)
                    .ThenInclude(t => t.History!)
                        .ThenInclude(h => h.User!)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            List<TicketHistory> ticketHistory = project!.Tickets!.SelectMany(t => t.History).ToList();

            return ticketHistory;
        }
        catch (Exception)
        {

            throw;
        }
    } 
    #endregion
}
