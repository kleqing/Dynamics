using Dynamics.Models.Models;
using System.Linq.Expressions;
using Dynamics.Utility;

namespace Dynamics.DataAccess.Repository
{
    public interface IUserRepository
    {
        Task<List<User?>> GetAllUsersAsync();
        IQueryable<User> GetUsersQueryable();
        Task<User?> GetAsync(Expression<Func<User?, bool>> filter);
        Task<bool> AddAsync(User? entity);
        Task<bool> UpdateAsync(User entity);
        Task<User> DeleteById(Guid id);
        Task<User?> GetUserProjectAsync(Expression<Func<User?, bool>> filter);
        Task<User?> GetUserProjectAsyncNoTracking(Expression<Func<User?, bool>> filter);
    }
}