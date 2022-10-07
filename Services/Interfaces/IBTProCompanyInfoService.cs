using BugTrackerPro.Models;

namespace BugTrackerPro.Services.Interfaces;

public interface IBTProCompanyInfoService
{
    Task<Company> GetCompanyInfoByIdAsync(int? companyId);

    Task<List<BTProUser>> GetAllMembersAsync(int? companyId);

    Task<List<Project>> GetAllProjectsAsync(int? companyId);

    Task<List<Ticket>> GetAllTicketsAsync(int? companyId);
}
