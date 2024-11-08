using Dynamics.DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Dynamics.Controllers;

public class NotificationController : Controller
{
    private readonly INotificationRepository _notifRepo;

    public NotificationController(INotificationRepository notifRepo)
    {
        _notifRepo = notifRepo;
    }

    [HttpPost]
    public async Task<JsonResult> MarkNotificationAsRead(Guid notificationId)
    {
        var notif = await _notifRepo.GetNotificationByIdAsync(notificationId);
        if (notif != null && notif.Status == 0)
        {
            notif.Status = 1;
            await _notifRepo.UpdateAsync(notif);
        }
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<JsonResult> MarkAllNotificationAsRead()
    {
        var userId = new Guid(HttpContext.Session.GetString("currentUserID"));
        if (userId != Guid.Empty)
        {
            await _notifRepo.MarkAllAsReadAsync(userId);
        }
        return Json(new { success = true });
    }

    [HttpPost]
    [Route("Notification/DeleteNotification")]
    public async Task<JsonResult> DeleteNotification(Guid notificationId)
    {
        var notif = await _notifRepo.GetNotificationByIdAsync(notificationId);
        if (notif != null)
        {
            await _notifRepo.DeleteAsync(notif);
        }
        return Json(new { success = true });
    }
}