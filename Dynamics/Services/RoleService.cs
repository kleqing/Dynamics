using System.Linq.Expressions;
using System.Web.Mvc;
using AutoMapper;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.Services;

public class RoleService : IRoleService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    
    
    public RoleService(UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager, 
        IUserRepository userRepository, IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<List<string>> GetRolesFromUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new Exception("ROLE: User not found");
        return await GetRolesFromUserAsync(user);
    }

    public async Task<List<string>> GetRolesFromUserAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    public async Task<List<UserVM>> GetUsersIncludingRoles(Expression<Func<User, bool>>? filter = null)
    {
        var users = new List<User>();
        if (filter != null)
        {
            users = await _userRepository.GetUsersQueryable().Where(filter).ToListAsync();
        }
        else users = await _userRepository.GetUsersQueryable().ToListAsync();
        var result = new List<UserVM>();
        foreach (var u in users)
        {
            var userVM = _mapper.Map<UserVM>(u);
            userVM.UserRoles = await _userManager.GetRolesAsync(u);
            result.Add(userVM);
        }
        return result;
    }

    public async Task AddUserToRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null) await AddUserToRoleAsync(user, roleName);
        else throw new Exception("ROLE: User not found");
    }

    public async Task AddUserToRoleAsync(User user, string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }

        await _userManager.AddToRoleAsync(user, roleName);
    }

    public async Task AddUserToRolesAsync(Guid userId, IEnumerable<string> roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null) await AddUserToRolesAsync(user, roleName);
        else throw new Exception("ROLE: User not found");
    }

    public async Task AddUserToRolesAsync(User user, IEnumerable<string> roleName)
    {
        await _userManager.AddToRolesAsync(user, roleName);
    }

    public async Task<bool> IsInRoleAsync(User user, string roleName)
    {
        return await _userManager.IsInRoleAsync(user, roleName);
    }

    public async Task<bool> IsInRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new Exception("ROLE: User not found");
        var result = await _userManager.IsInRoleAsync(user, roleName);
        return result;
    }

    public async Task DeleteRoleFromUserAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null) await DeleteRoleFromUserAsync(user, roleName);
        else throw new Exception("ROLE: User not found");
    }

    public async Task DeleteRoleFromUserAsync(User user, string roleName)
    {
        await _userManager.RemoveFromRoleAsync(user, roleName);
    }

    public async Task DeleteRolesFromUserAsync(Guid userId, IEnumerable<string> roleNames)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new Exception("ROLE: User not found");
        await DeleteRolesFromUserAsync(user, roleNames);
    }

    public async Task DeleteRolesFromUserAsync(User user, IEnumerable<string> roleNames)
    {
        await _userManager.RemoveFromRolesAsync(user, roleNames);
    }
}