using Dynamics.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.DataAccess.Repository
{
    public interface IOrganizationToProjectTransactionHistoryRepository
    {
        Task AddAsync(OrganizationToProjectHistory transaction); // Simple add to organizaiton
        Task<bool> AddOrgDonateRequestAsync(OrganizationToProjectHistory? orgDonate);

        Task<List<OrganizationToProjectHistory>> GetAllOrganizationDonateAsync(
            Expression<Func<OrganizationToProjectHistory, bool>> filter);

        //accept or deny transaction from org
        Task<bool> AcceptedOrgDonateRequestAsync(Guid transactionID);
        Task<bool> DenyOrgDonateRequestAsync(Guid transactionID);
    }
}