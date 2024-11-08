using Dynamics.Models.Models;

namespace Dynamics.Areas.Admin.Models
{
    public class Payment
    {
        public List<TransactionBase> viewwithdraw { get; set; }
        public List<UserToProjectTransactionHistory> listUserToProject { get; set; }
        public List<ProjectResource> listUserDonateToProjectTable { get; set; }
        public List<Withdraw> listWithdraws { get; set; }
        public Withdraw Withdraw { get; set; }
    }
}
