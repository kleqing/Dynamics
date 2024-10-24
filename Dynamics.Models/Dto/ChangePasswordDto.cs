using System.ComponentModel.DataAnnotations;

namespace Dynamics.Models
{
    public class ChangePasswordDto
    {
        public Guid UserId { get; set; }
        [DataType(DataType.Password)]
        [MaxLength(100, ErrorMessage = "The Password must be at least 6 and at max 100 characters long.")]
        [MinLength(6, ErrorMessage = "The Password must be at least 6 and at max 100 characters long.")]
        [Required]
        public string OldPassword { get; set; }
        [DataType(DataType.Password)]
        [MaxLength(100, ErrorMessage = "The Password must be at least 6 and at max 100 characters long.")]
        [MinLength(6, ErrorMessage = "The Password must be at least 6 and at max 100 characters long.")]
        [Required]
        public string NewPassword { get; set; }
        [DataType(DataType.Password)]
        [MaxLength(100, ErrorMessage = "The Password must be at least 6 and at max 100 characters long.")]
        [MinLength(6, ErrorMessage = "The Password must be at least 6 and at max 100 characters long.")]
        [Required]
        public string ConfirmPassword { get; set; }
    }
}
