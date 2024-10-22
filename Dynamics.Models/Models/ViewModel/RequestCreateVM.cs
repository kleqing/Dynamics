using System.ComponentModel.DataAnnotations;

namespace Dynamics.Models.Models.ViewModel;

public class RequestCreateVM
{
    //User
    public string UserEmail { get; set; }
    public string UserPhoneNumber { get; set; }
    public string UserAddress { get; set; }
    
    //Request
    [Required]
    [Display(Name = "Request title")]
    public string RequestTitle { get; set; }
    [Required]
    [Display(Name = "Request content")]
    public string Content { get; set; }
    public DateOnly? CreationDate { get; set; }
    [Required]
    public string RequestPhoneNumber { get; set; }
    
    public string RequestEmail { get; set; }
    [Required]
    public string Location { get; set; }
    public int isEmergency { get; set; }
}