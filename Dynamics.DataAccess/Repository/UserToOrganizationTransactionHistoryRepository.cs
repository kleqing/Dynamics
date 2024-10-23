using System.Linq.Expressions;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.DataAccess.Repository;

public class UserToOrganizationTransactionHistoryRepository : IUserToOrganizationTransactionHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public UserToOrganizationTransactionHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<UserToOrganizationTransactionHistory>> GetAllAsync()
    {
        return _context.UserToOrganizationTransactionHistories.ToListAsync();
    }

    public async Task<List<UserToOrganizationTransactionHistory>> GetAllAsyncWithExpression(
        Expression<Func<UserToOrganizationTransactionHistory, bool>> filter)
    {
        var test = await _context.UserToOrganizationTransactionHistories.Where(filter)
            .Include(ut => ut.User)
            .Include(ut => ut.OrganizationResource)
            .ThenInclude(ut => ut.Organization)
            .ToListAsync();
        test.FirstOrDefault(u => u.UserID == new Guid());
        return await _context.UserToOrganizationTransactionHistories.Where(filter)
            .Include(ut => ut.User)
            .Include(ut => ut.OrganizationResource)
            .ThenInclude(ut => ut.Organization)
            .ToListAsync();
    }

    public Task<UserToOrganizationTransactionHistory?> GetAsync(
        Expression<Func<UserToOrganizationTransactionHistory, bool>> filter)
    {
        return _context.UserToOrganizationTransactionHistories.Where(filter).FirstOrDefaultAsync();
    }

    public async Task AddAsync(UserToOrganizationTransactionHistory entity)
    {
        await _context.UserToOrganizationTransactionHistories.AddAsync(entity);
    }

    public Task<bool> Update(UserToOrganizationTransactionHistory entity)
    {
        throw new NotImplementedException();
    }

    public async Task<UserToOrganizationTransactionHistory> DeleteTransactionByIdAsync(Guid id)
    {
        var entity = await GetAsync(tr => tr.TransactionID.Equals(id));
        if (entity == null) return null;
        var final = _context.UserToOrganizationTransactionHistories.Remove(entity);
        await _context.SaveChangesAsync();
        return final.Entity;
    }
}