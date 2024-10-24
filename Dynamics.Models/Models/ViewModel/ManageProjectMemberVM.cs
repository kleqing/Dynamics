using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Models.ViewModel
{
    public class ManageProjectMemberVM
    {
        IEnumerable<ProjectMember> ProjectMembers { get; set; }
        IEnumerable<User> Users { get; set; }
    }
}
