using CloudinaryDotNet;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models;

namespace Dynamics.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifRepo;
    private readonly IHttpContextAccessor _accessor;
    private readonly IProjectMemberRepository _projectMemberRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUserToProjectTransactionHistoryRepository _utpTransRepo;
    private readonly IOrganizationToProjectTransactionHistoryRepository _otpTransRepo;
    private readonly IOrganizationMemberRepository _orgMemberRepo;

    public NotificationService(INotificationRepository notifRepo, 
        IHttpContextAccessor accessor, 
        IProjectMemberRepository projectMemberRepo, IUserRepository userRepo, IUserToProjectTransactionHistoryRepository utpTransRepo, IOrganizationToProjectTransactionHistoryRepository otpTransRepo, IOrganizationMemberRepository orgMemberRepo)
    {
        _notifRepo = notifRepo;
        _accessor = accessor;
        _projectMemberRepo = projectMemberRepo;
        _userRepo = userRepo;
        _utpTransRepo = utpTransRepo;
        _otpTransRepo = otpTransRepo;
        _orgMemberRepo = orgMemberRepo;
    }

    public async Task JoinProjectRequestNotificationAsync(Project project, string link)
    {
        var currentUserID = _accessor.HttpContext.Session.GetString("currentUserID");
        var notif = new Notification
        {
            NotificationID = Guid.NewGuid(),
            UserID = new Guid(currentUserID),
            Message = $"You have sent a join request to {project.ProjectName} project.",
            Date = DateTime.Now,
            Link = link,
            Status = 0 // Unread
        };
        await _notifRepo.AddNotificationAsync(notif);
    }

    public async Task ProcessJoinRequestNotificationAsync(Guid memberid, string link, string type)
    {
        switch (type)
        {
            case "join":
                var approveNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = memberid,
                    Message = "Your project joint request has been approved.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(approveNotif);
                break;
            case "deny":
                var denyNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = memberid,
                    Message = "Your project joint request has been denied.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(denyNotif);
                break;
        }
    }

    public async Task ProcessAllJoinRequestsNotificationAsync(Guid projectId, string link, string type)
    {
        var allJoinRequest =
            _projectMemberRepo.FilterProjectMember(
                p => p.ProjectID.Equals(projectId) && p.Status == 0);
        
        switch (type)
        {
            case "join":
                foreach (var joinRequest in allJoinRequest)
                {
                    var approveNotif = new Notification
                    {
                        NotificationID = Guid.NewGuid(),
                        UserID = joinRequest.UserID,
                        Message = "Your project joint request has been approved.",
                        Date = DateTime.Now,
                        Link = link,
                        Status = 0 // Unread
                    };
                    await _notifRepo.AddNotificationAsync(approveNotif);
                }
                break;
            case "deny":
                foreach (var joinRequest in allJoinRequest)
                {
                    var approveNotif = new Notification
                    {
                        NotificationID = Guid.NewGuid(),
                        UserID = joinRequest.UserID,
                        Message = "Your project joint request has been denied.",
                        Date = DateTime.Now,
                        Link = link,
                        Status = 0 // Unread
                    };
                    await _notifRepo.AddNotificationAsync(approveNotif);
                }
                break;
        }
    }

    public async Task DeleteProjectMemberNotificationAsync(Guid memberid, string link, string projectName)
    {
        var user = await _userRepo.GetAsync(u => u.UserID.Equals(memberid));
        var notif = new Notification
        {
            NotificationID = Guid.NewGuid(),
            UserID = user.UserID,
            Message = $"You have been removed from {projectName}.",
            Date = DateTime.Now,
            Link = link,
            Status = 0 // Unread
        };
        await _notifRepo.AddNotificationAsync(notif);
    }

    public async Task UpdateProjectNotificationAsync(Guid projectId, string link, string type, string? shutdownReason)
    {
        var allProjectMembers = await _projectMemberRepo.GetAllAsync(p => p.ProjectID.Equals(projectId));
        switch (type)
        { 
            case "update": 
                foreach (var member in allProjectMembers)
                { 
                    var notif = new Notification 
                    { 
                        NotificationID = Guid.NewGuid(),
                        UserID = member.UserID,
                        Message = $"One of your joined project: {member.Project.ProjectName} has been updated.",
                        Date = DateTime.Now,
                        Link = link,
                        Status = 0 // Unread
                    };
                    await _notifRepo.AddNotificationAsync(notif);
                }
                break;
            case "finish":
                foreach (var member in allProjectMembers)
                {
                    var notif = new Notification
                    {
                        NotificationID = Guid.NewGuid(),
                        UserID = member.UserID,
                        Message = $"Congratulation! Thanks to your contribution the project {member.Project.ProjectName} has successfully finished.",
                        Date = DateTime.Now,
                        Link = link,
                        Status = 0 // Unread
                    };
                    await _notifRepo.AddNotificationAsync(notif);
                }
                break;
            case "shutdown":
                foreach (var member in allProjectMembers)
                {
                    var notif = new Notification
                    {
                        NotificationID = Guid.NewGuid(),
                        UserID = member.UserID,
                        Message = $"One of your joined project: {member.Project.ProjectName} has been shutdown. Reason: {shutdownReason}.",
                        Date = DateTime.Now,
                        Link = link,
                        Status = 0 // Unread
                    };
                    await _notifRepo.AddNotificationAsync(notif);
                }
                break;
        }
    }

    public async Task ProcessProjectDonationNotificationAsync(Guid projectId, Guid? transId, string link, string type)
    {
        var projectLeader = await _projectMemberRepo.GetAsync(p => p.ProjectID.Equals(projectId) && p.Status == 2);
        var userDonate = await _utpTransRepo.GetAsync(utp => utp.TransactionID.Equals(transId));
        var orgDonate = await _otpTransRepo.GetAsync(otp => otp.TransactionID.Equals(transId));
        var orgLeader =
            await _orgMemberRepo.GetAsync(om =>
                om.OrganizationID.Equals(orgDonate.OrganizationResource.OrganizationID) && om.Status == 2);
        switch (type)
        {
            case "donate":
                var donateNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = projectLeader.UserID,
                    Message = $"A donation request has been sent project {projectLeader.Project.ProjectName} of yours.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(donateNotif);
                break;
            case "AcceptUserDonate":
                var acceptUserDonate = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = userDonate.UserID,
                    Message = $"Your donation to project {userDonate.ProjectResource.Project.ProjectName} has been accepted.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(acceptUserDonate);
                break;
            case "AcceptOrgDonate":
                var acceptOrgDonate = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = orgLeader.UserID,
                    Message = $"Donation to project {orgDonate.ProjectResource.Project.ProjectName} has been accepted.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(acceptOrgDonate);
                break;
            case "DenyUserDonate":
                var denyUserDonate = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = userDonate.UserID,
                    Message = $"Your donation to project {userDonate.ProjectResource.Project.ProjectName} has been denied. Reason: {userDonate.Message}",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(denyUserDonate);
                break;
            case "DenyOrgDonate":
                var denyOrgDonate = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = orgLeader.UserID,
                    Message = $"Donation to project {orgDonate.ProjectResource.Project.ProjectName} has been denied. Reason: {orgDonate.Message}",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(denyOrgDonate);
                break;
        }
    }

    public async Task ProcessProjectPhaseNotificationAsync(Guid projectId, string link, string type)
    {
        var allProjectMembers = await _projectMemberRepo.GetAllAsync(p => p.ProjectID.Equals(projectId));
        switch (type)
        {
            case "add":
                foreach (var member in allProjectMembers)
                {
                    var notif = new Notification
                    {
                        NotificationID = Guid.NewGuid(),
                        UserID = member.UserID,
                        Message = $"One of your joined project: {member.Project.ProjectName} has added a new phase.",
                        Date = DateTime.Now,
                        Link = link,
                        Status = 0 // Unread
                    };
                    await _notifRepo.AddNotificationAsync(notif);
                }
                break;
            case "update":
                foreach (var member in allProjectMembers)
                {
                    var notif = new Notification
                    {
                        NotificationID = Guid.NewGuid(),
                        UserID = member.UserID,
                        Message = $"One of your joined project: {member.Project.ProjectName} phase has been updated.",
                        Date = DateTime.Now,
                        Link = link,
                        Status = 0 // Unread
                    };
                    await _notifRepo.AddNotificationAsync(notif);
                }
                break;
            case "delete":
                foreach (var member in allProjectMembers)
                {
                    var notif = new Notification
                    {
                        NotificationID = Guid.NewGuid(),
                        UserID = member.UserID,
                        Message = $"One of your joined project: {member.Project.ProjectName} phase has been removed.",
                        Date = DateTime.Now,
                        Link = link,
                        Status = 0 // Unread
                    };
                    await _notifRepo.AddNotificationAsync(notif);
                }
                break;
        }
    }

    public async Task AddProjectResourceNotificationAsync(Guid projectId, string link)
    {
        var allProjectMembers = await _projectMemberRepo.GetAllAsync(p => p.ProjectID.Equals(projectId));
        foreach (var member in allProjectMembers)
        {
            var notif = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = member.UserID,
                Message = $"One of your joined project: {member.Project.ProjectName} has added a new resource. Do you want to donate now?",
                Date = DateTime.Now,
                Link = link,
                Status = 0 // Unread
            };
            await _notifRepo.AddNotificationAsync(notif);
        }
    }
}