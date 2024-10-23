using Dynamics.Models.Models;
using System.Linq.Expressions;
using Dynamics.Utility;

namespace Dynamics.DataAccess.Repository
{
    public interface IUserRepository
    {
        IQueryable<User> GetUsersQueryable();
        Task<User?> GetAsync(Expression<Func<User?, bool>> filter);
        Task<bool> AddAsync(User? entity);
        Task<bool> UpdateAsync(User entity);
        Task<User> DeleteById(Guid id);
        Task<User?> GetUserProjectAsync(Expression<Func<User?, bool>> filter);
        Task<User?> GetUserProjectAsyncNoTracking(Expression<Func<User?, bool>> filter);

        // Note: If want to check for role of an identity user, use user manager instead
        // Normal user can be checked by just calling the .UserRole method
        /**
         * Get role from a user in AUTH db
         */
        Task<string> GetRoleFromUserAsync(Guid userId);
        
        /**
         * Add a role to a user in BOTH Auth database and Main database
         * Note because a user only has one role, the older one will be deleted
         */
        Task AddToRoleAsync(Guid userId, string roleName);

        /**
         * Delete a role to a user in BOTH Auth database and Main database
         * Specify a 2nd param name role to make it the new role for the delete user
         * The default role after deletion is User
         */
        Task DeleteRoleFromUserAsync(Guid userId, string roleName = RoleConstants.User);
        //Task<bool> GetBanAsync(Guid userId);
    }
}