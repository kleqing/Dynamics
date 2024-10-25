using Dynamics.Models.Dto;
using Dynamics.Models.Models.ViewModel;
using Microsoft.AspNetCore.Http.Connections;
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

    public IQueryable<T> ApplyPaginateToQueryable<T>(IQueryable<T> query, PaginationRequestDto page) where T : class
    {
        return query
            .Skip((page.PageNumber - 1) * page.PageSize)
            .Take(page.PageSize);
    }

    public List<T> Paginate<T>(List<T> query, int pageNumber, int pageSize) where T : class
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public List<T> Paginate<T>(List<T> total, PaginationRequestDto paginationRequestDto, SearchRequestDto searchRequestDto, HttpContext context) where T : class
    {
        var currentFilter = context.Session.GetString("currentFilter");
        if (searchRequestDto.Filter != null)
        {
            if (currentFilter == null)
            {
                context.Session.SetString("currentFilter", searchRequestDto.Filter);
            }
            else if (!currentFilter.Equals(searchRequestDto.Filter, StringComparison.OrdinalIgnoreCase))
            {
                // If the is different from the last one, we reset the page number
                paginationRequestDto.PageNumber = 1;
            }
        }
        
        // Setup for display
        paginationRequestDto.TotalPages = (int)Math.Ceiling((double)(total.Count) / paginationRequestDto.PageSize);
        paginationRequestDto.TotalRecords = total.Count;
        return total
            .Skip((paginationRequestDto.PageNumber - 1) * paginationRequestDto.PageSize)
            .Take(paginationRequestDto.PageSize)
            .ToList();
    }
}