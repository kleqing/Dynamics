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
    private readonly IRequestRepository _reqRepo;

    public NotificationService(INotificationRepository notifRepo, 
        IHttpContextAccessor accessor, 
        IProjectMemberRepository projectMemberRepo, IUserRepository userRepo, IUserToProjectTransactionHistoryRepository utpTransRepo, IOrganizationToProjectTransactionHistoryRepository otpTransRepo, IOrganizationMemberRepository orgMemberRepo, IOrganizationRepository orgRepo, IUserToOrganizationTransactionHistoryRepository userToOrgTransRepo,
        IProjectRepository projectRepository, IOrganizationResourceRepository orgResourceRepo, IRequestRepository reqRepo)
    {
        _notifRepo = notifRepo;
        _accessor = accessor;
        _projectRepo = projectRepository;
        _orgResourceRepo = orgResourceRepo;
        _reqRepo = reqRepo;
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
            Message = $"Your request to join the \"{project.ProjectName}\" project has been sent successfully. You’ll be notified once your request is reviewed.",
            Date = DateTime.Now,
            Link = link,
            Status = 0 // Unread
        };
        await _notifRepo.AddNotificationAsync(notif);
    }
    public async Task InviteProjectMemberRequestNotificationAsync(Project projectObj, User member,User sender, string linkUser, string linkLeader)
    {
        var notif = new Notification
        {
            NotificationID = Guid.NewGuid(),
            UserID = member.Id,
            Message = $"You have been invited to join the \"{projectObj.ProjectName}\" project. Click here to view the project and accept the invitation.",
            Date = DateTime.Now,
            Link = linkUser,
            Status = 0 // Unread
        };
        await _notifRepo.AddNotificationAsync(notif);
        
        var notifLeader = new Notification
        {
            NotificationID = Guid.NewGuid(),
            UserID = sender.Id,
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
                    Message = $"You’ve invited \"{member.UserName}\" to join your project.",
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
                    Message = $"Your invitation to join the \"{project.ProjectName}\" project has been cancelled.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(cancelNotif);
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
                    Message = "Congratulations! Your request to join the project has been approved. Click here to get started.",
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
                    Message = "We regret to inform you that your request to join the project has been denied. Contact the project leader for more information.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(denyNotif);
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
            Message =  $"You have been removed from the project \"{projectName}\". Contact your project leader for more details.",
            Date = DateTime.Now,
            Link = link,
            Status = 0 // Unread
        };
        await _notifRepo.AddNotificationAsync(notif);
    }

    public async Task UpdateProjectNotificationAsync(Guid projectId, string link, string type, string? shutdownReason)
    {
        var allProjectMembers = await _projectMemberRepo.GetAllAsync(p => p.ProjectID.Equals(projectId));

        foreach (var member in allProjectMembers)
        {
            string message = type switch
            {
                "update" => $"An update has been made to the project \"{member.Project.ProjectName}\". Click here to view the changes.",
                "finish" => $"Congratulations! The project \"{member.Project.ProjectName}\" has been successfully completed. Thank you for your contribution.",
                "shutdown" => $"The project \"{member.Project.ProjectName}\" has been shut down. Reason: {shutdownReason}.",
                _ => throw new ArgumentException("Invalid notification type", nameof(type))
            };

            var notif = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = member.UserID,
                Message = message,
                Date = DateTime.Now,
                Link = link,
                Status = 0 // Unread
            };
            await _notifRepo.AddNotificationAsync(notif);
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
        var reason = "";
        switch (type)
        {
            case "donate":
                var donateNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = projectLeader.UserID,
                    Message = $"A donation request has been submitted for your project \"{project.ProjectName}\". Please review the request.",
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
                    Message = $"Your donation to the project \"{project.ProjectName}\" has been accepted. Thank you for your support!",
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
                    Message = $"Your organization’s donation to \"{project.ProjectName}\" has been accepted. Thank you for contributing!",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(acceptOrgDonate);
                break;
            case "DenyUserDonate":
                var userDonatee = await _utpTransRepo.GetAsync(utp => utp.TransactionID.Equals(transId));
                var indexOfReason = userDonatee.Message.IndexOf("Reason");
                if (indexOfReason != -1)
                {
                    // Extract the string starting from the first occurrence of "Reason"
                    reason = userDonatee.Message.Substring(indexOfReason);
                }
                var denyUserDonate = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = userDonatee.UserID,
                    Message = $"Your donation to the project \"{project.ProjectName}\" was not accepted. {reason}.",
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
                var indexOfReasonn = orgDonatee.Message.IndexOf("Reason");
                if (indexOfReasonn != -1)
                {
                    reason = orgDonatee.Message.Substring(indexOfReasonn);
                }
                var denyOrgDonate = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = orgLeaderr.UserID,
                    Message = $"Your organization’s donation to the project \"{project.ProjectName}\" was not accepted. {reason}.",
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

        foreach (var member in allProjectMembers)
        {
            var message = type switch
            {
                "add" => $"A new phase has been added to your project \"{member.Project.ProjectName}\". Check it out for more details.",
                "update" => $"A phase in your project \"{member.Project.ProjectName}\" has been updated. View the latest changes.",
                "delete" => $"A phase in your project \"{member.Project.ProjectName}\" has been removed.",
                _ => throw new ArgumentException("Invalid notification type", nameof(type))
            };

            var notif = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = member.UserID,
                Message = message,
                Date = DateTime.Now,
                Link = link,
                Status = 0 // Unread
            };
            await _notifRepo.AddNotificationAsync(notif);
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
                Message = $"A new resource has been added to your project \"{member.Project.ProjectName}\". Would you like to contribute a donation?",
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
                Message = $"Your organization, \"{member.Organization.OrganizationName}\", has been updated. Check out the latest changes.",
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
                    Message = $"A new donation request has been sent for your organization \"{organization.OrganizationName}\".",
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
                    Message = $"Your donation to \"{orgAccept.OrganizationName}\" has been accepted. Thank you for your support!",
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
                    Message = $"Unfortunately, your donation to \"{orgDeny.OrganizationName}\" has been declined.",
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
        var org = await _orgRepo.GetOrganizationAsync(o => o.OrganizationID.Equals(orgId));
        var orgMember = await _orgMemberRepo.GetAsync(o => o.OrganizationID.Equals(orgId) && o.UserID.Equals(userId));
        switch (type)
        {
            case "send":
                var sendNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = userId,
                    Message = orgMember.Status == 2
                        ? "Your organization has been successfully created."
                        : $"Your join request has been sent to \"{org.OrganizationName}\".",
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
                    Message = $"Your request to join \"{org.OrganizationName}\" has been approved. Welcome aboard!",
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
                    Message = $"Your request to join \"{org.OrganizationName}\" was denied.",
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
        var message = type == "left"
            ? $"You have successfully left the organization \"{org.OrganizationName}\"."
            : $"You were removed from the organization \"{org.OrganizationName}\".";
        var notif = new Notification
        {
            NotificationID = Guid.NewGuid(),
            UserID = userId,
            Message = message,
            Date = DateTime.Now,
            Link = link,
            Status = 0 // Unread
        };
        await _notifRepo.AddNotificationAsync(notif);
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
            Message = $"You have been appointed as the new CEO of \"{org.OrganizationName}\" by {oldCEO.UserName}. Congratulations!",
            Date = DateTime.Now,
            Link = link,
            Status = 0 // Unread
        };
        var oldCEONotif = new Notification
        {
            NotificationID = Guid.NewGuid(),
            UserID = oldCEOId,
            Message = $"You have successfully transferred the CEO position of \"{org.OrganizationName}\" to {newCEO.UserName}.",
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
                    Message = $"You have successfully added the resource \"{resource.ResourceName}\" to the \"{org.OrganizationName}\" organization.",
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
                    Message = $"You have successfully removed the resource \"{resource.ResourceName}\" from the \"{org.OrganizationName}\" organization.",
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
                    Message = $"Your organization has sent the resource \"{resource.ResourceName}\" to the project \"{project.ProjectName}\".",
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
                    Message = $"Your organization has unsent the resource \"{resource.ResourceName}\" to the project \"{project.ProjectName}\".",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(unsentNotif);
                break;
        }
    }

    public async Task AdminVerificationNotificationAsync(Guid id, string link, string type)
    {
        switch (type)
        {
            case "ApproveReq":
                var requestA = await _reqRepo.GetAsync(r => r.RequestID.Equals(id));
                var reqANotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = requestA.UserID,
                    Message = $"Your request \"{requestA.RequestTitle}\" has been approved by the admin.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(reqANotif);
                break;
            case "BanReq":
                var requestB = await _reqRepo.GetAsync(r => r.RequestID.Equals(id));
                var reqBNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = requestB.UserID,
                    Message = $"Your request \"{requestB.RequestTitle}\" has been canceled by the admin.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(reqBNotif);
                break;
            case "ApproveOrg":
                var leadA = await _orgMemberRepo.GetAsync(om => om.OrganizationID.Equals(id) && om.Status == 2);
                var orgANotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = leadA.UserID,
                    Message = "Your organization has been successfully verified by the admin.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(orgANotif);
                break;
            case "BanOrg":
                var leadB = await _orgMemberRepo.GetAsync(om => om.OrganizationID.Equals(id) && om.Status == 2);
                var orgBNotif = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = leadB.UserID,
                    Message = "Your organization has been banned by the admin.",
                    Date = DateTime.Now,
                    Link = link,
                    Status = 0 // Unread
                };
                await _notifRepo.AddNotificationAsync(orgBNotif);
                break;
        }
    }
}