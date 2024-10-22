using Dynamics.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Dynamics.Utility;

namespace Dynamics.DataAccess.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly AuthDbContext _authDbContext;
        private readonly UserManager<IdentityUser> _userManager;

        public UserRepository(ApplicationDbContext db, AuthDbContext authDbContext,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _authDbContext = authDbContext;
            this._userManager = userManager;
        }

        // TODO: Decide whether we use one database or 2 database for managing the user
        public async Task<bool> AddAsync(User? entity)
        {
            try
            {
                await _db.Users.AddAsync(entity);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<User> DeleteById(Guid id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.UserID.Equals(id));
            if (user != null)
            {
                // TODO NO NO DON'T Delete, BAN HIM INSTEAD
                // _db.Users.Remove(user);
                throw new Exception("TODO: BAN THIS USER INSTEAD");
                await _db.SaveChangesAsync();
            }

            return user;
        }
        public async Task<User?> GetUserProjectAsync(Expression<Func<User?, bool>> filter)
        {
            return await _db.Users.Include(u => u.ProjectMember).SingleOrDefaultAsync(filter);
        }
        
        public async Task<string> GetRoleFromUserAsync(Guid userId)
        {
            var authUser = await _userManager.FindByIdAsync(userId.ToString());
            if (authUser == null) throw new Exception("GET ROLE FAILED: USER NOT FOUND");
            return _userManager.GetRolesAsync(authUser).GetAwaiter().GetResult().FirstOrDefault();
        }

        // Add to both user side
        public async Task AddToRoleAsync(Guid userId, string roleName)
        {
            var authUser = await _userManager.FindByIdAsync(userId.ToString());
            var businessUser = await GetAsync(u => u.UserID == userId);
            if (authUser == null || businessUser == null) throw new Exception("ADD ROLE FAILED: USER NOT FOUND");
            businessUser.UserRole = roleName;
            // For identity, get the current role, delete it and add a new one
            var currentRole = _userManager.GetRolesAsync(authUser).GetAwaiter().GetResult().FirstOrDefault();
            if (currentRole != null)
            {
                await _userManager.RemoveFromRoleAsync(authUser, currentRole);
                await _userManager.AddToRoleAsync(authUser, roleName);
            }
            await _db.SaveChangesAsync();
        }

        public async Task DeleteRoleFromUserAsync(Guid userId, string roleName = RoleConstants.User)
        {
            var authUser = await _userManager.FindByIdAsync(userId.ToString());
            var businessUser = await GetAsync(u => u.UserID == userId);
            if (authUser == null || businessUser == null) throw new Exception("DELETE ROLE FAILED: USER NOT FOUND");
            var result = await _userManager.RemoveFromRoleAsync(authUser, roleName);
            businessUser.UserRole = roleName;
            await _db.SaveChangesAsync();
        }

        public async Task<User?> GetAsync(Expression<Func<User, bool>> filter)
        {
            var user = await _db.Users.Where(filter).FirstOrDefaultAsync();
            return user;
        }

        async Task<List<User?>> GetUsersByUserId(Expression<Func<User?, bool>> filter)
        {
            var users = await _db.Users.Where(filter).ToListAsync();
            return users;
        }

        public async Task<List<User?>> GetAllUsersAsync()
        {
            var users = await _db.Users.ToListAsync();
            return users;
        }

        //
        public async Task<bool> UpdateAsync(User user)
        {
            var existingItem = await GetAsync(u => user.UserID == u.UserID);
            if (existingItem == null)
            {
                return false;
            }

            // Things that identity might need to update: Name, Email
            var identityUser = await _userManager.FindByIdAsync(user.UserID.ToString());
            if (identityUser != null)
            {
                identityUser.UserName = user.UserFullName;
                identityUser.Email = user.UserEmail;
            }

            // Only update the property that has the same name between 2 models
            _db.Entry(existingItem).CurrentValues.SetValues(user);
            await _userManager.UpdateAsync(identityUser);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}