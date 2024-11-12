using Dynamics.Models.Models;

namespace Dynamics.Services;

public interface INotificationService
{
    Task JoinProjectRequestNotificationAsync(Project project, string link);
    Task ProcessJoinRequestNotificationAsync(Guid memberId, string link, string type);
    Task DeleteProjectMemberNotificationAsync(Guid memberId, string link, string projectName);
    Task UpdateProjectNotificationAsync(Guid projectId, string link, string type, string? shutdownReason);
    Task ProcessProjectDonationNotificationAsync(Guid projectId,Guid? transId, string link, string type);
    Task ProcessProjectPhaseNotificationAsync(Guid projectId, string link, string type);
    Task AddProjectResourceNotificationAsync(Guid projectId, string link);
    Task InviteProjectMemberRequestNotificationAsync(Project projectObj, User member, string linkUser, string linkLeader);
    Task ProcessInviteProjectMemberRequestNotificationAsync(Project project, User member, string link, string type);
    Task UpdateOrganizationNotificationAsync(Guid organizationId, string link);
    Task ProcessOrganizationDonationNotificationAsync(Guid organizationId, Guid? transId, string link, string type);
    Task ProcessOrganizationJoinRequestNotificationAsync(Guid userId, Guid orgId, string link, string type);
    Task ProcessOrganizationLeaveNotificationAsync(Guid userId, Guid orgId, string link, string type);
    Task TransferOrganizationCeoNotificationAsync(Guid newCEOId, Guid oldCEOId, Guid orgId, string link);
    Task ProcessOrganizationResourceNotificationAsync(Guid resourceId, string link, string type);
    Task OrganizationSendToProjectNotificationAsync(OrganizationResource resource, Guid projectId, string link, string type);
    Task AdminVerificationNotificationAsync(Guid id, string link, string type);
}