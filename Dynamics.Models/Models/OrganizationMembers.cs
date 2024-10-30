using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Models
{
    public class OrganizationMember
    {
		public Guid UserID { get; set; }
		public Guid OrganizationID { get; set; }
		public int? Status { get; set; }
		public virtual User User { get; set; }
		public virtual Organization Organization { get; set; }
	}
}
