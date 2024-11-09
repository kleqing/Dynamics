using System.Linq.Expressions;
using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.DataAccess.Repository;

public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _context;

    public WalletRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Wallet?> GetWalletAsync(Expression<Func<Wallet, bool>>? predicate = null)
    {
        return predicate == null ? await _context.Wallets.FirstOrDefaultAsync() : await _context.Wallets.FirstOrDefaultAsync(predicate);
    }

    public async Task<List<Wallet>> GetAllWalletAsync(Expression<Func<Wallet, bool>>? predicate  = null)
    {
        return predicate == null ? await _context.Wallets.ToListAsync() : await _context.Wallets.Where(predicate).ToListAsync();
    }

    public async Task<Wallet> CreateWalletAsync(Wallet wallet)
    {
        try
        {
            var created = await _context.Wallets.AddAsync(wallet);
            _context.SaveChangesAsync();
            return created.Entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task UpdateWalletAsync(Wallet wallet)
    {
        try
        {
            var existingWallet = await _context.Wallets.FindAsync(wallet.WalletId);
            if (existingWallet == null) throw new Exception("Wallet not found");
            existingWallet.Amount = wallet.Amount;
            existingWallet.Currency = wallet.Currency;
            
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task UpdateWalletAsync(Guid walletId, int amount, string? currency = null)
    {
        try
        {
            var existingWallet = await _context.Wallets.FindAsync(walletId);
            if (existingWallet == null) throw new Exception("Wallet not found");
            existingWallet.Amount = amount;
            existingWallet.Currency = currency ?? "VND";
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public IQueryable<Wallet> GetWalletAsQueryable(Expression<Func<Wallet, bool>>? predicate = null)
    {
        return predicate != null ? _context.Wallets.Where(predicate) : _context.Wallets;
    }
}