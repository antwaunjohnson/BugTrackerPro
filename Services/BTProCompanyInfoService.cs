﻿using BugTrackerPro.Data;
using BugTrackerPro.Models;
using BugTrackerPro.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerPro.Services;

public class BTProCompanyInfoService : IBTProCompanyInfoService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<BTProUser> _userManager;

    public BTProCompanyInfoService(ApplicationDbContext context, UserManager<BTProUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<BTProUser>> GetAllMembersAsync(int? companyId)
    {
        List<BTProUser> result = new();
        result = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
        return result;
    }

    public async Task<List<Project>> GetAllProjectsAsync(int? companyId)
    {
        List<Project> result = new();
        result = await _context.Projects!.Where(p => p.CompanyId == companyId)
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
        return result;
    }

    public async Task<List<Ticket>> GetAllTicketsAsync(int? companyId)
    {
        List<Ticket> result = new();
        List<Project> projects = new();

        projects = await GetAllProjectsAsync(companyId);

        result = projects.SelectMany(p => p.Tickets!).ToList();

        return result;
    }

    public async Task<Company> GetCompanyInfoByIdAsync(int? companyId)
    {
        Company? result = new();

        if(companyId != null)
        {
            result =  await _context.Companies!
                .Include(c => c.Members)
                .Include(c => c.Projects)
                .Include(c => c.Invites)
                .FirstOrDefaultAsync(c => c.Id == companyId);
        }
        return result!;
    }
}
