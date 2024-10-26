using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Models.ViewModel
{
    public class DetailProjectVM
    {
        public Project CurrentProject { get; set; }
        public User CurrentLeaderProject { get; set; }
        public double ProgressDonate { get; set; }
        public int ExpectedAmountOfMoneyDonate { get; set; }
        public int CurrentAmountOfMoneyDonate { get; set; }
        public int NumberOfProjectContributor { get; set; }
        public int TimeLeftEndDay { get; set; }
        public List<UserToProjectTransactionHistory> Random5Donors { get; set; }
    }
}
