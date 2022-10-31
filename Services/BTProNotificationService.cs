using BugTrackerPro.Data;
using BugTrackerPro.Models;
using BugTrackerPro.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerPro.Services;

public class BTProNotificationService : IBTProNotificationService
{

    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IBTProRolesService _roleService;

    public BTProNotificationService(ApplicationDbContext context, IEmailSender emailSender, IBTProRolesService roleService)
    {
        _context = context;
        _emailSender = emailSender;
        _roleService = roleService;
    }

    public async Task AddNotificationAsync(Notification notification)
    {
        try
        {
            await _context.AddAsync(notification);
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<List<Notification>> GetReceivedNotificationsAsync(string userId)
    {
        try
        {
            List<Notification> notifications = await _context.Notifications!
                .Include(n => n.Recipient)
                .Include(n => n.Sender)
                .Include(n => n.Ticket)
                    .ThenInclude(t => t!.Project)
                .Where(n => n.RecipientId == userId).ToListAsync();

            return notifications;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<List<Notification>> GetSentNotificationsAsync(string userId)
    {
        try
        {
            List<Notification> notifications = await _context.Notifications!
                .Include(n => n.Recipient)
                .Include(n => n.Sender)
                .Include(n => n.Ticket)
                    .ThenInclude(t => t!.Project)
                .Where(n => n.SenderId == userId).ToListAsync();

            return notifications;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject)
    {
        BTProUser? btpUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);

        if(btpUser != null)
        {
            string btpUserEmail = btpUser.Email;
            string message = notification.Message!;

            try
            {
                await _emailSender.SendEmailAsync(btpUserEmail, emailSubject, message);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
        else
        {
            return false;
        }
    }

    public async Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role)
    {
        try
        {
            List<BTProUser> members = await _roleService.GetUsersInRoleAsync(role, companyId);

            foreach (BTProUser btpUser  in members)
            {
                notification.RecipientId = btpUser.Id;
                await SendEmailNotificationAsync(notification, notification.Title!);
            }
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task SendMembersEmailNotificationsAsync(Notification notification, List<BTProUser> members)
    {
        try
        {
            foreach (BTProUser btpUser in members)
            {
                notification.RecipientId = btpUser.Id;
                await SendEmailNotificationAsync(notification, notification.Title!);
            }
        }
        catch (Exception)
        {

            throw;
        }
    }
}
