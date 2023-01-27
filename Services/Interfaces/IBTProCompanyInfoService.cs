using BugTrackerPro.Models;
using System.Runtime.CompilerServices;

namespace BugTrackerPro.Services.Interfaces;

public interface IBTProCompanyInfoService
{
    Task<Company> AddCompanyAsync(Company company);

    Task<Company> AddUserAsync(string Name, string Description);

    Task<Company> GetCompanyInfoByIdAsync(int? companyId);

    Task<List<BTProUser>> GetAllMembersAsync(int? companyId);

    Task<List<Project>> GetAllProjectsAsync(int? companyId);

    Task<List<Ticket>> GetAllTicketsAsync(int? companyId);
}
