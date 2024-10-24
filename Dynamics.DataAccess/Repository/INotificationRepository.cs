using Dynamics.Models.Models;

namespace Dynamics.DataAccess.Repository;

public interface INotificationRepository
{
    Task<List<Notification>> GetNotificationsAsync();
    Task AddNotificationAsync(Notification notification);
    Task<List<Notification>> GetCurrentUserNotificationsAsync(Guid userId);
    Task DeleteAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task<Notification> GetNotificationByIdAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
}