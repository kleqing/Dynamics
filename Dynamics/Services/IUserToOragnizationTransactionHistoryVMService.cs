using Dynamics.Models.Models;
using System.Linq.Expressions;

namespace Dynamics.Services
{
    public interface IUserToOragnizationTransactionHistoryVMService
    {

        Task<List<UserToOrganizationTransactionHistory>> GetTransactionHistory(Guid organizationId);

        Task<List<UserToOrganizationTransactionHistory>> GetTransactionHistoryIsAccept(Guid organizationId);

        Task<List<UserToOrganizationTransactionHistory>> GetTransactionHistory(Expression<Func<UserToOrganizationTransactionHistory, bool>> filter);

    }
}
