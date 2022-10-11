using BugTrackerPro.Models;

namespace BugTrackerPro.Services.Interfaces;

public interface IBTProTicketService
{
    Task AddNewTicketAsync(Ticket ticket);

    Task UpdateTicketAsync(Ticket ticket);

    Task<Ticket> GetTicketByIdAsync(int ticketId);

    Task ArchiveTicketAsync(Ticket ticket);



    Task AssignTicketAsync(int ticketId, string userId);

    Task<List<Ticket>> GetArchivedTicketsAsync(int companyId);

    Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId);

    Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName);

    Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName);

    Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName);

    Task<BTProUser> GetTicketDeveloperAsync(int ticketId);

    Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId,int companyId);

    Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId);

    Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId);

    Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId);

    Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId);

    Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId);



    Task<int?> LookupTicketPriorityIdAsync(string priorityName);

    Task<int?> LookupTicketStatusIdAsync(string statusName);

    Task<int?> LookupTicketTypeIdAsync(string typeName);
}
