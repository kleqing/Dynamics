using System.ComponentModel.DataAnnotations;

namespace Dynamics.Models.Dto;

public class VnPayResponseDto
{
    [Required]
    [Display(Name = "Money")]
    public int Amount { get; set; }
    public string? Message { get; set; }
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
    public DateTime Time { get; set; }
    public Guid TransactionID { get; set; } // We might not need this but whatever
    public bool Success { get; set; }
    public string VnPayResponseCode { get; set; }
}