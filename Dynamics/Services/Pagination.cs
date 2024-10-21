using Microsoft.EntityFrameworkCore;

namespace Dynamics.Services;

public class Pagination : IPagination
{
    public IQueryable<T> ToQueryable<T>(List<T> list) where T : class
    {
        return list.AsQueryable();
    }

    // To list async so that the query is executed
    public Task<List<T>> PaginateAsync<T>(IQueryable<T> query, int pageNumber, int pageSize) where T : class
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public List<T> Paginate<T>(List<T> query, int pageNumber, int pageSize) where T : class
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
}