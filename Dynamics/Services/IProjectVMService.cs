using Dynamics.Models.Models;
using System.Linq.Expressions;

namespace Dynamics.Services
{
    public interface IProjectVMService
    {
        Task<Project> GetProjectAsync(Expression<Func<Project, bool>> filter);
    }
}
