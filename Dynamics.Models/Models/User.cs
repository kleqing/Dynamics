using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dynamics.Models.Models
{
    public class User : IdentityUser<Guid>
    {
        //[Key]
        //public Guid Id { get; set; }
        //[Required]
        //[Display(Name = "Username")]
        //public string UserName { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateOnly? UserDOB { get; set; }
        //[Required]
        //[DataType(DataType.EmailAddress)]
        //[Display(Name = "Email Address")]
        //public string Email { get; set; }
        //[DataType(DataType.PhoneNumber)]
        //[Display(Name = "Phone Number")]
        //public string? PhoneNumber { get; set; }
        [ValidateNever]
        public string? UserAddress { get; set; }
        [ValidateNever]
        public string? UserAvatar { get; set; }
        public string? UserDescription { get; set; }
        // TODO: Remove them
        public string UserRole { get; set; }
        public bool isBanned { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        [NotMapped]
        public int ProjectCount { get; set; }
        [ValidateNever]
        // Self-referencing relationships for reports
        public virtual ICollection<Report> ReportsMade { get; set; }
        public virtual ICollection<Award> Award { get; set; }
        public virtual ICollection<Request> Request { get; set; }
        public virtual ICollection<ProjectMember> ProjectMember { get; set; }
        public virtual ICollection<OrganizationMember> OrganizationMember { get; set; }
        public virtual ICollection<UserToOrganizationTransactionHistory> UserToOrganizationTransactionHistories { get; set; }
        public virtual ICollection<UserToProjectTransactionHistory> UserToProjectTransactionHistories { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
