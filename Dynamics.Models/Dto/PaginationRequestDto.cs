namespace Dynamics.Models.Dto;

public class PaginationRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 9;
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public string TargetFormId { get; set; } // This property mostly used for the form on client side
}