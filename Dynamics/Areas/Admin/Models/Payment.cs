using Dynamics.Models.Models;

namespace Dynamics.Areas.Admin.Models
{
    public class Payment
    {
        public dynamic listTransaction { get; set; }
        public List<UserToProjectTransactionHistory> listUserToProject { get; set; }
        public List<OrganizationToProjectHistory> listOrganizationToProject { get; set; }
        public List<UserToOrganizationTransactionHistory> listUserToOrganization { get; set; }
        public List<ProjectResource> listUserDonateToProjectTable { get; set; }
        public List<OrganizationResource> listUserDonateToOrganizationTable { get; set; }
        public List<OrganizationResource> listOrganizationDonateToProjectTable { get; set; }
    }
}
