using System.Linq.Expressions;
using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.DataAccess.Repository
{
    public class UserWalletTransactionRepository : IUserWalletTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public UserWalletTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<UserWalletTransaction> GetUserWalletTransactionsQueryable(Expression<Func<UserWalletTransaction, bool>>? filter = null)
        {
            return filter == null ? _context.UserWalletTransactions : _context.UserWalletTransactions.Where(filter);
        }

        public async Task<UserWalletTransaction?> GetTransactionAsync(
            Expression<Func<UserWalletTransaction, bool>>? predicate = null)
        {
            return predicate == null
                ? await _context.UserWalletTransactions.FirstOrDefaultAsync()
                : await _context.UserWalletTransactions.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<UserWalletTransaction>> GetAllTransactionsAsync(
            Expression<Func<UserWalletTransaction, bool>>? predicate = null)
        {
            return predicate == null
                ? await _context.UserWalletTransactions.ToListAsync()
                : await _context.UserWalletTransactions.Where(predicate).ToListAsync();
        }

        public async Task<UserWalletTransaction> AddNewTransactionAsync(UserWalletTransaction transaction)
        {
            try
            {
                var created = await _context.UserWalletTransactions.AddAsync(transaction);
                await _context.SaveChangesAsync();
                return created.Entity;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /**
         * This is highly prohibited
         */
        public async Task UpdateTransactionAsync(UserWalletTransaction transaction)
        {
            try
            {
                var existingTransaction = await _context.UserWalletTransactions.FindAsync(transaction.TransactionId);
                if (existingTransaction == null) throw new Exception("Transaction not found");

                // Update the properties as necessary
                existingTransaction.Amount = transaction.Amount;
                existingTransaction.TransactionType = transaction.TransactionType;

                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /**
         * This also should never be used
         */
        public async Task DeleteTransactionAsync(Guid transactionId)
        {
            try
            {
                var transaction = await _context.UserWalletTransactions.FindAsync(transactionId);
                if (transaction == null) throw new Exception("Transaction not found");

                _context.UserWalletTransactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}