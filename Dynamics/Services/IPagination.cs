using Dynamics.Models.Dto;
using Dynamics.Models.Models.ViewModel;

namespace Dynamics.Services;

public interface IPagination
{
    IQueryable<T> ToQueryable<T>(List<T> list) where T : class;
    Task<List<T>> PaginateAsync<T>(IQueryable<T> query, int pageNumber, int pageSize) where T : class;
    /**
     * Return the queryable with paginated filter applied. <br/>
     * It is async because it needed to execute the query to get the rows
     */
    IQueryable<T> ApplyPaginateToQueryable<T>(IQueryable<T> query, PaginationRequestDto page) where T : class;
    /**
     * NOTE THIS ONE PAGINATE A LIST IN MEMORY, IT DOES NOT QUERY TO DATABASE OR SOMETHING
     */
    List<T> Paginate<T>(List<T> query, int pageNumber, int pageSize) where T : class;
    /**
     * Paginate an in memory list and setup the pagination requestDto as well
     */
    List<T> Paginate<T>(List<T> query, PaginationRequestDto paginationRequestDto, SearchRequestDto searchRequestDto, HttpContext context) where T : class;

}