using System.ComponentModel.DataAnnotations;

namespace Dynamics.Models.Dto
{
    public class ChangePasswordDto
    {
        public Guid UserId { get; set; }
        [DataType(DataType.Password)]
        [MaxLength(100, ErrorMessage = "The Password must be at least 6 and at max 100 characters long.")]
        [MinLength(6, ErrorMessage = "The Password must be at least 6 and at max 100 characters long.")]
        [Required]
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; }
        [DataType(DataType.Password)]
        [MaxLength(100, ErrorMessage = "The Password must be at least 6 and at max 100 characters long.")]
        [MinLength(6, ErrorMessage = "The Password must be at least 6 and at max 100 characters long.")]
        [Required]
        [Display(Name = "New Password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string NewPassword { get; set; }
        [DataType(DataType.Password)]
        [MaxLength(100, ErrorMessage = "The Password must be at least 6 and at max 100 characters long.")]
        [MinLength(6, ErrorMessage = "The Password must be at least 6 and at max 100 characters long.")]
        [Required]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
