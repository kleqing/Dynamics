using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Dynamics.Models.Dto;

public class VnPayRequestDto
{
    // These guys will be used for updating our database
    public Guid FromID { get; set; } // Can be from either user or organization and can be null because organization allocation does not need it
    public Guid TransactionID { get; set; } // Auto generated
    public Guid ResourceID { get; set; } // The destination ResourceID (If allocation, it will be organizationResource id)
    [Required(ErrorMessage = "Please provide a destination to donate")]
    public Guid TargetId { get; set; } // The target organization / project
    public string TargetType { get; set; } // Is the donation to organization or project or allocation
    public int Status { get; set; } // This should always be 1 bc money is automatic
    // The rest will be used for display purposes
    [Required]
    [Display(Name = "Money")]
    public int Amount { get; set; }
    public string? Message { get; set; }
    [DataType(DataType.DateTime)]
    public DateTime Time { get; set; }
}