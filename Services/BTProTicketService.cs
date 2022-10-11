using BugTrackerPro.Data;
using BugTrackerPro.Models;
using BugTrackerPro.Services.Interfaces;

namespace BugTrackerPro.Services;

public class BTProTicketService : IBTProTicketService
{

    private readonly ApplicationDbContext _context;
    private readonly IBTProProjectService _projectService;
    private readonly IBTProRolesService _roleService;

    public BTProTicketService(ApplicationDbContext context,
                              IBTProProjectService projectService,
                              IBTProRolesService roleService)
    {
        _context = context;
        _projectService = projectService;
        _roleService = roleService;
    }

    public Task AddNewTicketAsync(Ticket ticket)
    {
        throw new NotImplementedException();
    }

    public Task ArchiveTicketAsync(Ticket ticket)
    {
        throw new NotImplementedException();
    }

    public Task AssignTicketAsync(int ticketId, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
    {
        throw new NotImplementedException();
    }

    public Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
    {
        throw new NotImplementedException();
    }

    public Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
    {
        throw new NotImplementedException();
    }

    public Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
    {
        throw new NotImplementedException();
    }

    public Task<Ticket> GetTicketByIdAsync(int ticketId)
    {
        throw new NotImplementedException();
    }

    public Task<BTProUser> GetTicketDeveloperAsync(int ticketId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
    {
        throw new NotImplementedException();
    }

    public Task<int?> LookupTicketPriorityIdAsync(string priorityName)
    {
        throw new NotImplementedException();
    }

    public Task<int?> LookupTicketStatusIdAsync(string statusName)
    {
        throw new NotImplementedException();
    }

    public Task<int?> LookupTicketTypeIdAsync(string typeName)
    {
        throw new NotImplementedException();
    }

    public Task UpdateTicketAsync(Ticket ticket)
    {
        throw new NotImplementedException();
    }
}
