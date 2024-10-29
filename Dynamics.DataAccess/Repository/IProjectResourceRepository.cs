using System.Linq.Expressions;
using Dynamics.Models.Models;

namespace Dynamics.DataAccess.Repository;

public interface IProjectResourceRepository
{
    Task<ProjectResource?> GetAsync(Expression<Func<ProjectResource, bool>> predicate);

    //handle resource statistic-huyen
    public IQueryable<ProjectResource> GetAllProjectResourceQuery();
    Task<bool> HandleResourceAutomatic(Guid transactionID, string donor);
    Task<IEnumerable<ProjectResource?>> FilterProjectResourceAsync(Expression<Func<ProjectResource, bool>> filter);
    Task<bool> AddResourceTypeAsync(ProjectResource entity);
    Task<bool> UpdateResourceTypeAsync(ProjectResource entity);

    Task<bool> DeleteResourceTypeAsync(Guid resourceID);
    Task UpdateAsync(ProjectResource entity);
}