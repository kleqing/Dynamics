using Dynamics.Models.Models;
using Dynamics.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dynamics.DataAccess.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public UserRepository(ApplicationDbContext db,
            UserManager<User> userManager)
        {
            _db = db;
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
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id.Equals(id));
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

        public async Task<User?> GetUserProjectAsyncNoTracking(Expression<Func<User?, bool>> filter)
        {
            return await _db.Users.Include(u => u.ProjectMember).AsNoTracking().SingleOrDefaultAsync(filter);
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
            var businessUser = await GetAsync(u => u.Id == userId);
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
            var businessUser = await GetAsync(u => u.Id == userId);
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

        public IQueryable<User> GetUsersQueryable()
        {
            return _db.Users;
        }

        //
        public async Task<bool> UpdateAsync(User user)
        {
            var existingItem = await GetAsync(u => user.Id == u.Id);
            if (existingItem == null)
            {
                return false;
            }
            // For other update method, create different repo method.
            existingItem.UserName = user.UserName;
            existingItem.Email = user.Email;
            existingItem.PhoneNumber = user.PhoneNumber;
            existingItem.UserAddress = user.UserAddress;
            existingItem.UserDOB = user.UserDOB;
            existingItem.UserDescription = user.UserDescription;

            // Only update the property that has the same name between 2 models
            //_db.Entry(existingItem).CurrentValues.SetValues(user);
            //await _userManager.UpdateAsync(existingItem);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<List<User?>> GetAllUsersAsync()
        {
            var users = await _db.Users.ToListAsync();
            return users;
        }
    }
}