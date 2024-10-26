using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.DataAccess.Repository
{
     public class ProjectHistoryRepository: IProjectHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        //-----------------manage project update----------------------------
        public async Task<List<History>?> GetAllPhaseReportsAsync(Expression<Func<History, bool>> filter)
        {
            IQueryable<History> updates = _context.Histories.Include(x => x.Project).Where(filter);
            if (updates != null)
            {
                return await updates.ToListAsync();

            }
            return null;
        }

        public async Task<bool> AddPhaseReportAsync(History entity)
        {
            if (entity == null) return false;

            await _context.Histories.AddAsync(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditPhaseReportAsync(History entity)
        {
            if (entity == null) return false;
            var existingItem = await _context.Histories.FirstOrDefaultAsync(x => x.HistoryID.Equals(entity.HistoryID));
            if (existingItem == null)
            {
                return false;
            }

            // Detach the entity from the context if it's already being tracked
            _context.Entry(existingItem).State = EntityState.Detached;

            // Now update the existing entity with new values
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePhaseReportAsync(Guid id)
        {
            var deleteItem = await _context.Histories.FirstOrDefaultAsync(x => x.HistoryID.Equals(id));
            if (deleteItem == null)
            {
                return false;
            }

            _context.Histories.Remove(deleteItem);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
