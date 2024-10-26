
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using System.Linq.Expressions;

namespace Dynamics.Services
{
    public interface IOrganizationVMService
    {
        Task<OrganizationVM> GetOrganizationVMAsync(Expression<Func<Organization, bool>> filter);
        Task<List<OrganizationVM>> GetAllOrganizationVMsAsync();
        Task<List<OrganizationVM>> GetOrganizationVMsByUserIDAsync(Guid userId);
        Task<OrganizationVM> GetOrganizationVMByUserIDAsync(Guid userId);
    }
}
