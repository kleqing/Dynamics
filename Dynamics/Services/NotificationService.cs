using AutoMapper.Execution;
using CloudinaryDotNet;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Project = Dynamics.Models.Models.Project;

namespace Dynamics.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifRepo;
    private readonly IHttpContextAccessor _accessor;
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectMemberRepository _projectMemberRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUserToProjectTransactionHistoryRepository _utpTransRepo;
    private readonly IOrganizationToProjectTransactionHistoryRepository _otpTransRepo;
    private readonly IOrganizationMemberRepository _orgMemberRepo;
    private readonly IOrganizationRepository _orgRepo;
    private readonly IUserToOrganizationTransactionHistoryRepository _userToOrgTransRepo;
    private readonly IOrganizationResourceRepository _orgResourceRepo;

    public NotificationService(INotificationRepository notifRepo, 
        IHttpContextAccessor accessor, 
        IProjectMemberRepository projectMemberRepo, IUserRepository userRepo, IUserToProjectTransactionHistoryRepository utpTransRepo, IOrganizationToProjectTransactionHistoryRepository otpTransRepo, IOrganizationMemberRepository orgMemberRepo, IOrganizationRepository orgRepo, IUserToOrganizationTransactionHistoryRepository userToOrgTransRepo,
        IProjectRepository projectRepository, IOrganizationResourceRepository orgResourceRepo)
    {
        _notifRepo = notifRepo;
        _accessor = accessor;
        _projectRepo = projectRepository;
        _orgResourceRepo = orgResourceRepo;
        _projectMemberRepo = projectMemberRepo;
        _userRepo = userRepo;
        _utpTransRepo = utpTransRepo;
        _otpTransRepo = otpTransRepo;
        _orgMemberRepo = orgMemberRepo;
        _orgRepo = orgRepo;
        _userToOrgTransRepo = userToOrgTransRepo;
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
    public async Task InviteProjectMemberRequestNotificationAsync(Project projectObj, User member, string linkUser, string linkLeader)
    {
        var notif = new Notification
        {
            NotificationID = Guid.NewGuid(),
            UserID = member.Id,
            Message = $"Project {projectObj.ProjectName} has sent you an invitation to be their project member",
            Date = DateTime.Now,
            Link = linkUser,
            Status = 0 // Unread
        };
        await _notifRepo.AddNotificationAsync(notif);

        var leaderOfProject = await GetProjectLeaderAsync(projectObj.ProjectID);
        var notifLeader = new Notification
        {
            NotificationID = Guid.NewGuid(),
            UserID = leaderOfProject.Id,
            Message = $"You have sent an invitation to \"{member.UserName}\" to join your project {projectObj.ProjectName}.\nClick this notification to cancel the invitation!",
            Date = DateTime.Now,
            Link = linkLeader,
            Status = 0 // Unread
        };
        await _notifRepo.AddNotificationAsync(notifLeader);
    }
    public async Task ProcessInviteProjectMemberRequestNotificationAsync(Project project, User member, string link, string type)
    {
        var leaderOfProject = await GetProjectLeaderAsync(project.ProjectID);
        switch (type)
        {
            case "join":
                var existed = _notifRepo.GetNotificationAsync(n => n.UserID.Equals(leaderOfProject.Id) && n.Link.Equals(link));
                if (existed != null) break;
                var approveNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = leaderOfProject.Id,
                    Message = $"{member.UserName} has accepted your invitation",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(approveNotif);
                break;
            case "cancel":
                var cancelNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = member.Id,
                    Message = $"Your invitation to join the project {project.ProjectName} has been cancelled",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(cancelNotif);
                break;
            case "deny":
                var denyNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = leaderOfProject.Id,
                    Message = $"{member.UserName} has denied your invitation",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(denyNotif);
                break;
        }
    }
    public async Task<User> GetProjectLeaderAsync(Guid projectID)
    {
        var projectObj = await _projectRepo.GetProjectAsync(x => x.ProjectID.Equals(projectID));
        ProjectMember leaderProjectMembers = projectObj?.ProjectMember.Where(x => x.Status == 3).FirstOrDefault();
        //if no leader then leader is the ceo of organization
        if (leaderProjectMembers == null)
        {
            leaderProjectMembers = projectObj?.ProjectMember.Where(x => x.Status == 2).FirstOrDefault();
        }

        if (leaderProjectMembers != null)
        {
            return leaderProjectMembers?.User;
        }

        return null;
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
        var user = await _userRepo.GetAsync(u => u.Id.Equals(memberid));
        var notif = new Notification
        {
            NotificationID = Guid.NewGuid(),
            UserID = user.Id,
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
        var projectLeader = await _projectMemberRepo.GetAsync(p => p.ProjectID.Equals(projectId) && p.Status == 3);
        if (projectLeader == null)
        {
            projectLeader = await _projectMemberRepo.GetAsync(p => p.ProjectID.Equals(projectId) && p.Status == 2);
        }
        var project = await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectId));
        switch (type)
        {
            case "donate":
                var donateNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = projectLeader.UserID,
                    Message = $"A donation request has been sent to project {projectLeader.Project.ProjectName} of yours.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(donateNotif);
                break;
            case "AcceptUserDonate":
                var userDonate = await _utpTransRepo.GetAsync(utp => utp.TransactionID.Equals(transId));
                var acceptUserDonate = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = userDonate.UserID,
                    Message = $"Your donation to project {project.ProjectName} has been accepted.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(acceptUserDonate);
                break;
            case "AcceptOrgDonate":
                var orgDonate = await _otpTransRepo.GetAsync(otp => otp.TransactionID.Equals(transId));
                var orgLeader =
                    await _orgMemberRepo.GetAsync(om =>
                        om.OrganizationID.Equals(orgDonate.OrganizationResource.OrganizationID) && om.Status == 2);
                var acceptOrgDonate = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = orgLeader.UserID,
                    Message = $"Your organization donation to project {project.ProjectName} has been accepted.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(acceptOrgDonate);
                break;
            case "DenyUserDonate":
                var userDonatee = await _utpTransRepo.GetAsync(utp => utp.TransactionID.Equals(transId));
                var denyUserDonate = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = userDonatee.UserID,
                    Message = $"Your donation to project {project.ProjectName} has been denied. Reason: {userDonatee.Message}",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(denyUserDonate);
                break;
            case "DenyOrgDonate":
                var orgDonatee = await _otpTransRepo.GetAsync(otp => otp.TransactionID.Equals(transId));
                var orgLeaderr =
                    await _orgMemberRepo.GetAsync(om =>
                        om.OrganizationID.Equals(orgDonatee.OrganizationResource.OrganizationID) && om.Status == 2);
                var denyOrgDonate = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = orgLeaderr.UserID,
                    Message = $"Your organization donation to project {project.ProjectName} has been denied. Reason: {orgDonatee.Message}",
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

    public async Task UpdateOrganizationNotificationAsync(Guid organizationId, string link)
    {
        var allOrgMembers = await _orgMemberRepo.GetAllAsync(p => p.OrganizationID.Equals(organizationId));
        foreach (var member in allOrgMembers)
        {
            var notif = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = member.UserID,
                Message = $"One of your joined organization: {member.Organization.OrganizationName} has been updated.",
                Date = DateTime.Now,
                Link = link,
                Status = 0 // Unread
            };
            await _notifRepo.AddNotificationAsync(notif);
        }
    }

    public async Task ProcessOrganizationDonationNotificationAsync(Guid organizationId, Guid? transId, string link, string type)
    {
        switch (type)
        {
            case "donate":
                var orgLeader = await _orgMemberRepo.GetAsync(p => p.OrganizationID.Equals(organizationId) && p.Status == 2);
                var organization = await _orgRepo.GetOrganizationAsync(o => o.OrganizationID.Equals(organizationId));
                var donateNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = orgLeader.UserID,
                    Message = $"A donation request has been sent project {organization.OrganizationName} of yours.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(donateNotif);
                break;
            case "accept":
                var trans = await _userToOrgTransRepo.GetAsync(uto => uto.TransactionID.Equals(transId));
                var orgAccept = await _orgRepo.GetOrganizationAsync(o => o.OrganizationID.Equals(organizationId));
                var acceptNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = trans.UserID,
                    Message = $"Your donation to project {orgAccept.OrganizationName} has been accepted.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(acceptNotif);
                break;
            case "deny":
                var transDeny = await _userToOrgTransRepo.GetAsync(uto => uto.TransactionID.Equals(transId));
                var orgDeny = await _orgRepo.GetOrganizationAsync(o => o.OrganizationID.Equals(organizationId));
                var denyNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = transDeny.UserID,
                    Message = $"Your donation to project {orgDeny.OrganizationName} has been denied.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(denyNotif);
                break;
        }
    }

    public async Task ProcessOrganizationJoinRequestNotificationAsync(Guid userId, Guid orgId, string link, string type)
    {
        switch (type)
        {
            case "send":
                var org = await _orgRepo.GetOrganizationAsync(o => o.OrganizationID.Equals(orgId));
                var sendNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = userId,
                    Message = $"You have sent a join request to {org.OrganizationName} organization.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(sendNotif);
                break;
            case "join":
                var approveNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = userId,
                    Message = "Your organization join request has been approved.",
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
                    UserID = userId,
                    Message = "Your organization joint request has been denied.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(denyNotif);
                break;
        }
    }

    public async Task ProcessOrganizationLeaveNotificationAsync(Guid userId, Guid orgId, string link, string type)
    {
        var org = await _orgRepo.GetOrganizationAsync(o => o.OrganizationID.Equals(orgId));
        switch (type)
        {
            case "left":
                var leftNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = userId,
                    Message = $"You had left organization {org.OrganizationName}.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(leftNotif);
                break;
            case "remove":
                var removeNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = userId,
                    Message = $"You have been removed from {org.OrganizationName} organization.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(removeNotif);
                break;
        }
    }

    public async Task TransferOrganizationCeoNotificationAsync(Guid newCEOId, Guid oldCEOId, Guid orgId, string link)
    {
        var org = await _orgRepo.GetOrganizationAsync(o => o.OrganizationID.Equals(orgId));
        var newCEO = await _userRepo.GetAsync(u => u.Id.Equals(newCEOId));
        var oldCEO = await _userRepo.GetAsync(u => u.Id.Equals(oldCEOId));
        var newCEONotif = new Notification
        {
            NotificationID = Guid.NewGuid(),
            UserID = newCEOId,
            Message = $"You have been appointed to be {org.OrganizationName} organization CEO by {oldCEO.UserName}.",
            Date = DateTime.Now,
            Link = link,
            Status = 0 // Unread
        };
        var oldCEONotif = new Notification
        {
            NotificationID = Guid.NewGuid(),
            UserID = oldCEOId,
            Message = $"You have transferred your CEO position from {org.OrganizationName} organization to {newCEO.UserName}.",
            Date = DateTime.Now,
            Link = link,
            Status = 0 // Unread
        };
        await _notifRepo.AddNotificationAsync(newCEONotif);
        await _notifRepo.AddNotificationAsync(oldCEONotif);
    }

    public async Task ProcessOrganizationResourceNotificationAsync(Guid resourceId, string link, string type)
    {
        var resource = await _orgResourceRepo.GetAsync(r => r.ResourceID.Equals(resourceId));
        var orgLeader = await _orgMemberRepo.GetAsync(om => om.OrganizationID.Equals(resource.OrganizationID) && om.Status == 2);
        var org = await _orgRepo.GetOrganizationAsync(o => o.OrganizationID.Equals(resource.OrganizationID));
        switch (type)
        {
            case "add":
                var addNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = orgLeader.UserID,
                    Message = $"You have successfully added {resource.ResourceName} to {org.OrganizationName} organization.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(addNotif);
                break;
            case "remove":
                var removeNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = orgLeader.UserID,
                    Message = $"You have successfully removed {resource.ResourceName} from {org.OrganizationName} organization.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(removeNotif);
                break;
        }
    }

    public async Task OrganizationSendToProjectNotificationAsync(OrganizationResource resource, Guid projectId, string link, string type)
    {
        var orgLeader = await _orgMemberRepo.GetAsync(om => om.OrganizationID.Equals(resource.OrganizationID) && om.Status == 2);
        var project = await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectId));
        switch (type)
        {
            case "sent":
                var sentNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = orgLeader.UserID,
                    Message = $"Your organization have sent {resource.ResourceName} to {project.ProjectName} project.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(sentNotif);
                break;
            case "unsent":
                var unsentNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = orgLeader.UserID,
                    Message = $"Your organization have successfully unsent {resource.ResourceName} from {project.ProjectName} project.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(unsentNotif);
                break;
        }
    }
}