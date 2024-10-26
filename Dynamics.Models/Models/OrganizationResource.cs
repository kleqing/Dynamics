using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Models
{
    public class OrganizationResource
    {
		public Guid ResourceID { get; set; }
		public Guid OrganizationID { get; set; }
        [Required(ErrorMessage = "The Resource Name field is required *")]
        public string ResourceName { get; set; }
		public int Quantity { get; set; }

        [Required(ErrorMessage = "The Unit field is required *")]
		public string Unit { get; set; }
        public virtual Organization Organization { get; set; }
		public virtual ICollection<UserToOrganizationTransactionHistory> UserToOrganizationTransactionHistory { get; set; }
		public virtual ICollection<OrganizationToProjectHistory> OrganizationToProjectHistory { get;set; }
	}
}
