using Dynamics.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.DataAccess.Repository
{
    public interface IReportRepository
    {
        //manage report of a project
        Task<bool> SendReportProjectRequestAsync(Report entity);
    }
}
