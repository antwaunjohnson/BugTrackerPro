using BugTrackerPro.Models;

namespace BugTrackerPro.Services.Interfaces;

public interface IBTProNotificationService
{
    Task AddNotificationAsync(Notification notification);

    Task<List<Notification>> GetReceivedNotificationsAsync(string userId);

    Task<List<Notification>> GetSentNotificationsAsync(string userId);

    Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role);

    Task SendMembersEmailNotificationsAsync(Notification notification, List<BTProUser> members);

    Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject);
}
