using BugTrackerPro.Data;
using BugTrackerPro.Models;
using BugTrackerPro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerPro.Services;

public class BTProInviteService : IBTProInviteService
{

    private readonly ApplicationDbContext _context;

    public BTProInviteService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Accept Invite
    public async Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId)
    {
        Invite? invite = await _context.Invites!.FirstOrDefaultAsync(i => i.CompanyToken == token);

        if (invite == null)
        {
            return false;
        }

        try
        {
            invite.IsValid = false;
            invite.InviteeId = userId;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception)
        {

            throw;
        }
    }
    #endregion

    #region Add New Invite
    public async Task AddNewInviteAsync(Invite invite)
    {
        try
        {
            await _context.Invites!.AddAsync(invite);
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {

            throw;
        }
    }
    #endregion

    #region Any Invites
    public async Task<bool> AnyInvitesAsync(Guid token, string email, int companyId)
    {
        try
        {
            bool result = await _context.Invites!.Where(i => i.CompanyId == companyId)
                                        .AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email);

            return result;
        }
        catch (Exception)
        {

            throw;
        }
    }
    #endregion

    #region Get Invite(invitId)
    public async Task<Invite> GetInviteAsync(int inviteId, int companyId)
    {
        try
        {
            Invite? invite = await _context.Invites!.Where(i => i.CompanyId == companyId)
                                  .Include(i => i.Company)
                                  .Include(i => i.Project)
                                  .Include(i => i.Invitor)
                                  .FirstOrDefaultAsync(i => i.Id == inviteId);

            return invite!;
        }
        catch (Exception)
        {

            throw;
        }
    }
    #endregion

    #region Get Invite(token)
    public async Task<Invite> GetIviteAsync(Guid token, string email, int companyId)
    {
        try
        {
            Invite? invite = await _context.Invites!.Where(i => i.CompanyId == companyId)
                   .Include(i => i.Company)
                   .Include(i => i.Project)
                   .Include(i => i.Invitor)
                   .FirstOrDefaultAsync(i => i.CompanyToken == token && i.InviteeEmail == email);

            return invite!;
        }
        catch (Exception)
        {

            throw;
        }
    }
    #endregion

    #region Validate Invite
    public async Task<bool> ValidateIviteCodeAsync(Guid? token)
    {
        if (token == null)
        {
            return false;
        }

        bool result = false;

        Invite? invite = await _context.Invites!.FirstOrDefaultAsync(i => i.CompanyToken == token);

        if (invite != null)
        {
            DateTime inviteDate = invite.InviteDate.DateTime;

            bool validDate = (DateTime.Now - inviteDate).TotalDays <= 7;

            if (validDate)
            {
                result = invite.IsValid;
            }
        }
        return result;
    } 
    #endregion
}
