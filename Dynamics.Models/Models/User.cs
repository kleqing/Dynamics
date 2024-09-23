﻿using System.ComponentModel.DataAnnotations;

namespace Dynamics.Models.Models
{
    public class User
    {
        
        public string UserID { get; set; }
        public string UserFullName { get; set; }
        [DataType(DataType.Date)]
        public DateOnly? UserDOB { get; set; }
        [DataType(DataType.EmailAddress)]
        public string UserEmail { get; set; }
        [DataType(DataType.PhoneNumber)]        
        public string? UserPhoneNumber { get; set; }
        public string? UserAddress { get; set; }
        public string? UserAvatar { get; set; }
        public string? UserDescription { get; set; }
        public virtual ICollection<Award> Award { get; set; }
        public virtual ICollection<Request> Request { get; set; }
        public virtual ICollection<ProjectMember> ProjectMember { get; set; }
        public virtual ICollection<OrganizationMember> OrganizationMember { get; set; }
        public virtual ICollection<UserToOrganizationTransactionHistory> UserToOrganizationTransactions { get; set; }
        public virtual ICollection<UserToProjectTransactionHistory> UserToProjectTransactions { get; set; }
    }
}
