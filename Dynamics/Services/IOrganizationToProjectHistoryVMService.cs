using Dynamics.Models.Models;

namespace Dynamics.Services
{
    public interface IOrganizationToProjectHistoryVMService
    {

        Task<List<OrganizationToProjectHistory>> GetAllOrganizationToProjectHistoryAsync(Guid organizationId);
        Task<List<OrganizationToProjectHistory>> GetAllOrganizationToProjectHistoryByAcceptingAsync(Guid organizationId);

    }
}
