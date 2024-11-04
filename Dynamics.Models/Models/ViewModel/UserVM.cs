using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dynamics.Models.Models.ViewModel;
/**
 * This one is the same as user model but with role column for extract
 */
public class UserVM
{
    public Guid Id { get; set; }
    [Required]
    [Display(Name = "Username")]
    [MaxLength(50)]
    public string UserName { get; set; }
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateOnly? UserDOB { get; set; }
    [Required]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Email Address")]
    public string Email { get; set; }
    [Display(Name = "Phone Number")]
    [DataType(DataType.PhoneNumber)]
    [StringLength(11, ErrorMessage = "The phone number must be at most 11 digits.")]
    [RegularExpression(@"^\d{1,11}$", ErrorMessage = "The phone number must only contain digits")]
    public string? PhoneNumber { get; set; }
    [ValidateNever]
    public string? UserAddress { get; set; }
    [ValidateNever]
    public string? UserAvatar { get; set; }
    [ValidateNever]
    [MaxLength(1000)]
    public string? UserDescription { get; set; }
    [ValidateNever]
    public IEnumerable<string> UserRoles { get; set; }
    public bool isBanned { get; set; }
    [ValidateNever]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime? CreatedDate { get; set; } = DateTime.Now;
    [ValidateNever]
    [NotMapped]
    public int ProjectCount { get; set; }
}