using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.DataAccess.Repository;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _db;

    public NotificationRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Notification>> GetNotificationsAsync()
    {
        return await _db.Notifications.Include(n => n.User).ToListAsync();
    }

    public async Task AddNotificationAsync(Notification notification)
    {
        await _db.Notifications.AddAsync(notification);
    }

    public async Task<List<Notification>> GetCurrentUserNotificationsAsync(Guid userId)
    {
        return await _db.Notifications.Where(n => n.UserID == userId).ToListAsync();
    }

    public async Task DeleteAsync(Notification notification)
    {
        var notif = await _db.Notifications.FirstOrDefaultAsync(n => n.NotificationID == notification.NotificationID);
        if (notif != null)
        {
            _db.Notifications.Remove(notif);
            await _db.SaveChangesAsync();
        }
    }

    public async Task UpdateAsync(Notification notification)
    {
        var notif = await _db.Notifications.FirstOrDefaultAsync(n => n.NotificationID == notification.NotificationID);
        if (notif != null)
        {
            _db.Notifications.Update(notif);
            await _db.SaveChangesAsync();
        }
    }
}