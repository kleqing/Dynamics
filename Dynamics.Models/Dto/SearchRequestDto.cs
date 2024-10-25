using System.ComponentModel.DataAnnotations;

namespace Dynamics.Models.Models.ViewModel;

public class SearchRequestDto
{
    public string? Query { get; set; }
    public string? Filter { get; set; }
    [DataType(DataType.Date)]
    public DateOnly? DateFrom { get; set; }
    [DataType(DataType.Date)]
    public DateOnly? DateTo { get; set; }
}