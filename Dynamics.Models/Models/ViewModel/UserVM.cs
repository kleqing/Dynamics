using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Dynamics.Models.Models.ViewModel;
/**
 * This one is the same as user model but with role column for extract
 */
public class UserVM
{
    [Required]
    [Display(Name = "Username")]
    [MaxLength(50)]
    public string UserFullName { get; set; }
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateOnly? UserDOB { get; set; }
    [Required]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Email Address")]
    public string UserEmail { get; set; }
    [DataType(DataType.PhoneNumber)]
    [Display(Name = "Phone Number")]
    [MaxLength(11)]
    public string? UserPhoneNumber { get; set; }
    [ValidateNever]
    public string? UserAddress { get; set; }
    [ValidateNever]
    public string? UserAvatar { get; set; }
    [MaxLength(1000)]
    public string? UserDescription { get; set; }
    public IEnumerable<string> UserRoles { get; set; }
    public bool isBanned { get; set; }
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime? CreatedDate { get; set; } = DateTime.Now;
    [NotMapped]
    public int ProjectCount { get; set; }
}