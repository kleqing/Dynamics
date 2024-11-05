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

    public async Task<List<T>> PaginateAsync<T>(IQueryable<T> query, HttpContext context, PaginationRequestDto? paginationRequestDto = null,
        SearchRequestDto? searchRequestDto = null) where T : class
    {
        var pages = query.Count();
        var total = await query
                                .Skip((paginationRequestDto.PageNumber - 1) * paginationRequestDto.PageSize)
                                .Take(paginationRequestDto.PageSize)
                                .ToListAsync();
        var currentFilter = context.Session.GetString("currentFilter");
        if (searchRequestDto != null && searchRequestDto.Filter != null)
        {
            if (currentFilter != null && !currentFilter.Equals(searchRequestDto.Filter, StringComparison.OrdinalIgnoreCase))
            {
                // If the filter is different from the last one, we reset the page number
                paginationRequestDto.PageNumber = 1;
            }
            context.Session.SetString("currentFilter", searchRequestDto.Filter);
        }
        
        // Setup for display
        paginationRequestDto.TotalPages = (int)Math.Ceiling((double)(pages) / paginationRequestDto.PageSize);
        paginationRequestDto.TotalRecords = pages;
        return total;
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

    public List<T> Paginate<T>(List<T> total, HttpContext context, PaginationRequestDto? paginationRequestDto = null, 
        SearchRequestDto? searchRequestDto = null) where T : class
    {
        var currentFilter = context.Session.GetString("currentFilter");
        if (searchRequestDto != null && searchRequestDto.Filter != null)
        {
            if (currentFilter != null && !currentFilter.Equals(searchRequestDto.Filter, StringComparison.OrdinalIgnoreCase))
            {
                // If the filter is different from the last one, we reset the page number
                paginationRequestDto.PageNumber = 1;
            }
            context.Session.SetString("currentFilter", searchRequestDto.Filter);
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