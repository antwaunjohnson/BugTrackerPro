using BugTrackerPro.Data;
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

    public async Task<Company> AddCompanyAsync(Company company)
    {
        try
        {
            await _context.AddAsync(company);
            await _context.SaveChangesAsync();
            return company;
        }   
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Company> AddUserAsync(string Name)
    {
        Name = Name.ToLower();

        List<Company> companies = await _context.Companies!.ToListAsync();

        foreach (Company company in companies)
        {
            if (company.Name == Name)
            {
                return company;
            }
        }

        Company newCompany = new();

        newCompany.Name = Name;

        await AddCompanyAsync(newCompany);

        return newCompany;
    }

    #region Get All Members
    public async Task<List<BTProUser>> GetAllMembersAsync(int? companyId)
    {
        List<BTProUser> result = new();
        result = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
        return result;
    }

    #endregion

    #region Get All Projects
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
    #endregion

    #region Get All Tickets
    public async Task<List<Ticket>> GetAllTicketsAsync(int? companyId)
    {
        List<Ticket> result = new();
        List<Project> projects = new();

        projects = await GetAllProjectsAsync(companyId);

        result = projects.SelectMany(p => p.Tickets!).ToList();

        return result;
    }
    #endregion

    #region Get Company Info By Id
    public async Task<Company> GetCompanyInfoByIdAsync(int? companyId)
    {
        Company? result = new();

        if (companyId != null)
        {
            result = await _context.Companies!
                .Include(c => c.Members)
                .Include(c => c.Projects)
                .Include(c => c.Invites)
                .FirstOrDefaultAsync(c => c.Id == companyId);
        }
        return result!;
    } 
    #endregion
}
