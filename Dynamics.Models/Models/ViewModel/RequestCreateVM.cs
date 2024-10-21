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
    public string RequestTitle { get; set; }
    [Required]
    public string Content { get; set; }
    public DateOnly? CreationDate { get; set; }
    [Required]
    public string RequestPhoneNumber { get; set; }
    [Required]
    public string RequestEmail { get; set; }
    [Required]
    public string Location { get; set; }
    public int isEmergency { get; set; }
}