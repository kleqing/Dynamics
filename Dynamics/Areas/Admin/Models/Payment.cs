using Dynamics.Models.Models;

namespace Dynamics.Areas.Admin.Models
{
    public class Payment
    {
        public List<UserToProjectTransactionHistory> listUserToProject { get; set; }
        public List<OrganizationToProjectHistory> listOrganizationToProject { get; set; }
        public List<UserToOrganizationTransactionHistory> listUserToOrganization { get; set; }
    }
}
