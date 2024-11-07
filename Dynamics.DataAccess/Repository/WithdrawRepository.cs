using System.Linq.Expressions;
using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.DataAccess.Repository;

public class WithdrawRepository : IWithdrawRepository
{
    private readonly ApplicationDbContext _db;

    public WithdrawRepository(ApplicationDbContext db)
    {
        _db = db;
    }
    public async Task AddAsync(Withdraw entity)
    {
        await _db.Withdraws.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<Withdraw?> GetWithdraw(Expression<Func<Withdraw, bool>> filer)
    {
        return await _db.Withdraws.Where(filer)
            .Include(u => u.Project)
            .ThenInclude(pr => pr.ProjectResource)
            .ThenInclude(p => p.UserToProjectTransactionHistory)
            .FirstOrDefaultAsync();
    }
}