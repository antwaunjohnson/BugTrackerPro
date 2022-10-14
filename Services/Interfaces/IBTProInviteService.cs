using BugTrackerPro.Models;

namespace BugTrackerPro.Services.Interfaces;

public interface IBTProInviteService
{
    Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId);

    Task AddNewInviteAsync(Invite invite);

    Task<bool> AnyInvitesAsync(Guid token, string email, int companyId);

    Task<Invite> GetInviteAsync(int inviteId, int companyId);

    Task<Invite> GetIviteAsync(Guid token, string email, int companyId);

    Task<bool> ValidateIviteCodeAsync(Guid? token);
}
