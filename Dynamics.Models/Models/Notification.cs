using System.ComponentModel.DataAnnotations;

namespace Dynamics.Models.Models;

public class Notification
{
    public Guid NotificationID { get; set; }
    public Guid UserID { get; set; }
    public string Message { get; set; }
    [DataType(DataType.DateTime)]
    public DateTime Date { get; set; }
    public string Link { get; set; }
    // 0 -> unread, 1 -> read
    public int Status { get; set; }
    public virtual User User { get; set; }
}