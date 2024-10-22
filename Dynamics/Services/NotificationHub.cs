using Dynamics.DataAccess;
using Dynamics.DataAccess.Repository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.Services;

public class NotificationHub : Hub
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IUserRepository _userRepo;
    private readonly ApplicationDbContext _db;

    public NotificationHub(INotificationRepository notificationRepo, ApplicationDbContext db, IUserRepository userRepo)
    {
        _notificationRepo = notificationRepo;
        _db = db;
        _userRepo = userRepo;
    }

    public override async Task<Task> OnConnectedAsync()
    {
        try
        {
            var user = await _userRepo.GetUserProjectAsync(u => u.UserFullName == Context.User.Identity.Name);
            
            // store the connection id in the session
            Context.GetHttpContext().Session.SetString($"{user.UserID.ToString()}_signalr", Context.ConnectionId);
            // add user to group base on ProjectID
            // foreach (var item in user.ProjectMember)
            // {
            //     await Groups.AddToGroupAsync(Context.ConnectionId, item.ProjectID.ToString());
            // }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"ConnectedAsync error: {ex.Message}");
        }
        return base.OnConnectedAsync();
    }
    
    // send message to a specific group
    public async Task SendGroupNotification(string groupid, string message)
    {
        await Clients.Group(groupid).SendAsync("ReceiveNotification", message);
    }
    // send message to current user
    public async Task SendClientNotification(string message)
    {
        await Clients.Client(Context.ConnectionId).SendAsync("ReceiveNotification", message);
    }
    // add current user to group
    public async void AddToGroupAsync(string groupid)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupid);
    }
}