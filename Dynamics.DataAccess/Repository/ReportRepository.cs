using Dynamics.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.DataAccess.Repository
{
    public class ReportRepository:IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> SendReportProjectRequestAsync(Report entity)
        {
            if (entity == null) return false;
            entity.ReportID = Guid.NewGuid();
            entity.ReportDate = DateTime.Now;
            await _context.Reports.AddAsync(entity);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
