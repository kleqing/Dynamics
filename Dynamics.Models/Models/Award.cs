using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Models
{
    public class Award
    {
        public Guid AwardID { get; set; }
        public string AwardName { get; set; }
        public Guid UserID { get; set; }
        public virtual User User { get; set; }
    }
}
