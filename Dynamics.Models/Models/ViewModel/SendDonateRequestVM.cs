using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamics.Models.Dto;

namespace Dynamics.Models.Models.ViewModel
{
    public class SendDonateRequestVM
    {
        public Guid ProjectID { get; set; }
        public string TypeDonor { get; set; }
        public Guid OrganizationUserLeadID { get; set; }
        public UserToProjectTransactionHistory? UserDonate { get; set; }
        public OrganizationToProjectHistory? OrgDonate { get; set; }
        public List<UserToProjectTransactionHistory>? UserTransactionHistory { get; set; }
        public List<OrganizationToProjectHistory>? OrgTransactionHistory { get; set; }
        public VnPayRequestDto VnPayRequestDto { get; set; }
    }
}
