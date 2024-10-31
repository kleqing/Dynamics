using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dynamics.DataAccess;
using Dynamics.Models.Models;
using Dynamics.DataAccess.Repository;
using Dynamics.Utility;
using Microsoft.AspNetCore.Authorization;
using Dynamics.Areas.Admin.Models;

namespace Dynamics.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    [Area("Admin")]
    public class PaymentsController : Controller
    {

        private readonly IAdminRepository _adminRepository;

        public PaymentsController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(RoleConstants.Admin))
            {
                var userTransactionToProject = await _adminRepository.ViewUserToProjectTransactionInHistory(u => true);
                var organizationTransactionToProjects = await _adminRepository.ViewOrganizationToProjectTransactionHistory(o => true);
                var userTransactionToOrganization = await _adminRepository.ViewUserToOrganizationTransactionHistory(u => true);

                var allTransaction = new List<TransactionBase>();
                
                allTransaction.AddRange(userTransactionToProject.Select(p => new TransactionBase
                {
                    TransactionID = p.TransactionID,
                    Time = p.Time,
                    Message = p.Message,
                    Type = "UserToProject",
                    Sender = p.User.UserName
                }));
                
                allTransaction.AddRange(organizationTransactionToProjects.Select(p => new TransactionBase
                {
                    TransactionID = p.TransactionID,
                    Time = p.Time,
                    Message = p.Message,
                    Type = "OrganizationToProject",
                    Sender = p.OrganizationResource.Organization.OrganizationName
                }));
                
                allTransaction.AddRange(userTransactionToOrganization.Select(p => new TransactionBase
                {
                    TransactionID = p.TransactionID,
                    Time = p.Time,
                    Message = p.Message,
                    Type = "UserToOrganization",
                    Sender = p.User.UserName
                }));
                
                var sortedTransaction = allTransaction.OrderByDescending(t => t.Time).ToList();
                
                var model = new Payment
                {
                    listTransaction = sortedTransaction
                };
                
                return View(model);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Details(Guid? id, string Type)
        {
            if (User.IsInRole(RoleConstants.Admin))
            {
                if (Type == "UserToProject")
                {
                    var userToProject = await _adminRepository.ViewUserToProjectTransactionInHistory(u => u.TransactionID == id);
                    if (userToProject == null || !userToProject.Any())
                    {
                        return NotFound();
                    }

                    var projectID = userToProject.FirstOrDefault().ProjectResource.Project.ProjectID;
                    
                    var listResource = await _adminRepository.ViewUserToProjectResource(p => p.ProjectID == projectID);

                    var userToProjectdetail = userToProject.FirstOrDefault();
                    
                    var model = new Payment
                    {
                        listUserToProject = new List<UserToProjectTransactionHistory> { userToProjectdetail },
                        listUserDonateToProjectTable = listResource
                    };
                    return View("Details", model);
                }

                else if (Type == "OrganizationToProject")
                {
                    var organizationToProject = await _adminRepository.ViewOrganizationToProjectTransactionHistory(o => o.TransactionID == id);
                    if (organizationToProject == null || !organizationToProject.Any())
                    {
                        return NotFound();
                    }

                    var orgID = organizationToProject.FirstOrDefault().OrganizationResource.Organization.OrganizationID;

                    var listResource = await _adminRepository.ViewOrganizationToProjectResource(i => i.OrganizationID == orgID);
                    
                    var orgToProjectdetail = organizationToProject.FirstOrDefault();

                    var model = new Payment
                    {
                        listOrganizationToProject = new List<OrganizationToProjectHistory> { orgToProjectdetail },
                        listOrganizationDonateToProjectTable = listResource
                    };
                    return View("Details", model);
                }
                
                else if (Type == "UserToOrganization")
                {
                    var userToOrganization =
                        await _adminRepository.ViewUserToOrganizationTransactionHistory(o => o.TransactionID == id);
                    if (userToOrganization == null || !userToOrganization.Any())
                    {
                        return NotFound();
                    }
                    
                    var orgID = userToOrganization.FirstOrDefault().OrganizationResource.Organization.OrganizationID;
                    
                    var listResource = await _adminRepository.ViewUserDonateOrganizationResource(o => o.OrganizationID == orgID);

                    var userToOrgDetail = userToOrganization.FirstOrDefault();
                    
                    var model = new Payment
                    {
                        listUserToOrganization = new List<UserToOrganizationTransactionHistory>() { userToOrgDetail },
                        listUserDonateToOrganizationTable = listResource
                    };
                    return View("Details", model);
                }
                
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
