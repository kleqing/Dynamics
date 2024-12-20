﻿using Dynamics.Models.Models;
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
            _db.Entry(user).State = EntityState.Detached;
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
            existingItem.UserAvatar = user.UserAvatar;
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