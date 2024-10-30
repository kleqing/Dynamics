using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Models
{
    public class ProjectMember
    {
		public Guid UserID { get; set; }
		public Guid ProjectID { get; set; }
		public int Status { get; set; }
		public virtual User User { get; set; }
		public virtual Project Project { get; set; }
	}
}
