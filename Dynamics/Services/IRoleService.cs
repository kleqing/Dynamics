using System.Linq.Expressions;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;

namespace Dynamics.Services;
public interface IRoleService
{
    /**
     * Get all roles from a user
     */
    Task<List<string>> GetRolesFromUserAsync(Guid userId);
    Task<List<string>> GetRolesFromUserAsync(User user);
    /**
     * Get a list of user VM, which will contain the role they have <br/>
     * The role property will be a list in case user has multiple roles <br/>
     * Use expression to determine the user list, also this one does not include anything
     */
    Task<List<UserVM>> GetUsersIncludingRoles(Expression<Func<User, bool>>? filter = null);
    Task AddUserToRoleAsync(Guid userId, string roleName);
    Task AddUserToRoleAsync(User user, string roleName);
    /**
     * Add multiple role to a user
     */
    Task AddUserToRolesAsync(Guid userId, IEnumerable<string> roleName);
    Task AddUserToRolesAsync(User user, IEnumerable<string> roleName);
    /**
     * Check if a user is in a role <br/>
     * If user has multiple roles, check multiple times
     */
    Task<bool> IsInRoleAsync(User user, string roleName);
    Task<bool> IsInRoleAsync(Guid userId, string roleName);
    
    /**
     * Delete role(s) from user
     */
    Task DeleteRoleFromUserAsync(Guid userId, string roleName);
    Task DeleteRoleFromUserAsync(User user, string roleName);
    Task DeleteRolesFromUserAsync(Guid userId, IEnumerable<string> roleName);
    Task DeleteRolesFromUserAsync(User user, IEnumerable<string> roleName);

}