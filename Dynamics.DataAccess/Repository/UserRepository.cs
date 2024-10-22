using Dynamics.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dynamics.DataAccess.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> userManager;

        public UserRepository(ApplicationDbContext db, AuthDbContext authDbContext, UserManager<IdentityUser> userManager)
        {
            _db = db;
            this.userManager = userManager;
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
            var user = await _db.Users.FirstOrDefaultAsync(x=>x.UserID.Equals(id));
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

        public async Task<User?> GetAsync(Expression<Func<User?, bool>> filter)
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
            // Only update the property that has the same name between 2 models
            _db.Entry(existingItem).CurrentValues.SetValues(user);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
