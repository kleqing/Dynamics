using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamics.Models.Dto;

namespace Dynamics.Models.Models.ViewModel
{
    public class ProjectTransactionHistoryVM
    {
        // public List<UserToProjectTransactionHistory> UserDonate { get; set; }
        // public List<OrganizationToProjectHistory> OrganizationDonate { get; set; }
        public List<UserTransactionDto> Transactions { get; set; }
        public SearchRequestDto SearchRequestDto { get; set; }
        public PaginationRequestDto PaginationRequestDto { get; set; }
        public readonly string[] FilterOptions = { "Filter", "Organization", "User", "Denied", "Accepted" };
    }
}
