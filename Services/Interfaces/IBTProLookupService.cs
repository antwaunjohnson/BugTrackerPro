using BugTrackerPro.Models;

namespace BugTrackerPro.Services.Interfaces;

public interface IBTProLookupService
{
    public Task<List<TicketPriority>> GetTicketPrioritiesAsync();

    public Task<List<TicketStatus>> GetTicketStatusesAsync();

    public Task<List<TicketType>> GetTicketTypesAsync();

    public Task<List<ProjectPriority>> GetProjectPrioritiesAsync();
}
