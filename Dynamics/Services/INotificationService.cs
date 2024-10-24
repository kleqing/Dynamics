using Dynamics.Models.Models;

namespace Dynamics.Services;

public interface INotificationService
{
    Task JoinProjectRequestNotificationAsync(Project project, string link);
    Task ProcessJoinRequestNotificationAsync(Guid memberId, string link, string type);
    Task ProcessAllJoinRequestsNotificationAsync(Guid projectId, string link, string type);
    Task DeleteProjectMemberNotificationAsync(Guid memberId, string link, string projectName);
    Task UpdateProjectNotificationAsync(Guid projectId, string link, string type, string? shutdownReason);
    Task ProcessProjectDonationNotificationAsync(Guid projectId,Guid? transId, string link, string type);
    Task ProcessProjectPhaseNotificationAsync(Guid projectId, string link, string type);
    Task AddProjectResourceNotificationAsync(Guid projectId, string link);
    Task UpdateOrganizationNotificationAsync(Guid organizationId, string link);
    Task ProcessOrganizationDonationNotificationAsync(Guid organizationId, Guid? transId, string link, string type);
}