using Dynamics.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.DataAccess.Repository
{
    public interface IProjectHistoryRepository
    {
        //manage project update
        Task<List<History>?> GetAllPhaseReportsAsync(Expression<Func<History, bool>> filter);

        Task<bool> AddPhaseReportAsync(History entity);

        Task<bool> EditPhaseReportAsync(History entity);

        Task<bool> DeletePhaseReportAsync(Guid id);
    }
}
