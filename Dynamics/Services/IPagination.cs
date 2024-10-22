namespace Dynamics.Services;

public interface IPagination
{
    IQueryable<T> ToQueryable<T>(List<T> list) where T : class;
    Task<List<T>> PaginateAsync<T>(IQueryable<T> query, int pageNumber, int pageSize) where T : class;
    /**
     * NOTE THIS ONE PAGINATE A LIST IN MEMORY, IT DOES NOT QUERY TO DATABASE OR SOMETHING
     */
    List<T> Paginate<T>(List<T> query, int pageNumber, int pageSize) where T : class;

}