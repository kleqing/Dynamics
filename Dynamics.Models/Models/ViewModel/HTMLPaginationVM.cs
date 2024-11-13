using Dynamics.Models.Dto;

namespace Dynamics.Models.Models.ViewModel;

public class HTMLPaginationVM
{
    // Note that a page will only have one kind of search going on
    // The form that the pagination will target
    public string? FormId { get; set; } = "searchForm";
    // The pagination footer
    public string? PaginationDivId { get; set; } = "pDiv";
    public PaginationRequestDto PaginationRequestDto { get; set; }
}