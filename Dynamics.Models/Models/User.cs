using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Dynamics.Models.Models
{
    public class User : IdentityUser<Guid>
    {
        //[Key]
        //public Guid Id { get; set; }
        //[Required]
        //[Display(Name = "Username")]
        [MaxLength(50)] public override string? UserName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateOnly? UserDOB { get; set; }

        //[Required]
        //[DataType(DataType.EmailAddress)]
        //[Display(Name = "Email Address")]
        //public string Email { get; set; }
        //[DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(11, ErrorMessage = "The phone number must be at most 11 digits.")]
        [RegularExpression(@"^\d{1,11}$", ErrorMessage = "The phone number must only contain digits")]
        public override string? PhoneNumber { get; set; }

        [ValidateNever] public string? UserAddress { get; set; }
        [ValidateNever] public string? UserAvatar { get; set; }

        [MaxLength(1000)] public string? UserDescription { get; set; }

        // TODO: Remove them
        public string UserRole { get; set; }
        public bool isBanned { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [NotMapped] public int ProjectCount { get; set; }

        // public Guid WalletId { get; set; } = Guid.Empty;                         
        [ValidateNever]
        // Self-referencing relationships for reports
        public virtual ICollection<Report> ReportsMade { get; set; }

        [ValidateNever] public virtual ICollection<Award> Award { get; set; }
        [ValidateNever] public virtual ICollection<Request> Request { get; set; }
        [ValidateNever] public virtual ICollection<ProjectMember> ProjectMember { get; set; }
        [ValidateNever] public virtual ICollection<OrganizationMember> OrganizationMember { get; set; }
        [ValidateNever]
        public virtual ICollection<UserToOrganizationTransactionHistory> UserToOrganizationTransactionHistories { get; set; }

        [ValidateNever]
        public virtual ICollection<UserToProjectTransactionHistory> UserToProjectTransactionHistories { get; set; }

        [ValidateNever] public virtual ICollection<Notification> Notifications { get; set; }
        [ValidateNever] public virtual Wallet Wallet { get; set; }
    }
}