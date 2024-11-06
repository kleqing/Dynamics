﻿﻿using Dynamics.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dynamics.Models.Models.ViewModel;

namespace Dynamics.DataAccess.Repository
{
    public interface IAdminRepository
    {
        // 2024-9-27
        // Request
        Task<List<Request>> ViewRequest();
        Task<Request?> GetRequest(Expression<Func<Request, bool>> filter);
        Task<int> ChangeRequestStatus(Guid id);
        Task<Request> DeleteRequest(Guid id);
        // 2024-10-17
        Task<Request> GetRequestInfo(Expression<Func<Request, bool>> expression);

        // User   
        Task<List<UserVM>> ViewUser();
        Task<bool> BanUserById(Guid id);
        Task<List<User>> GetTop5User();
        Task<User?> GetUser(Expression<Func<User, bool>> filter);
        Task ChangeUserRole(Guid id);
        Task<string> GetUserRole(Guid id);


        // Organization
        Task<List<Organization>> ViewOrganization();
        Task<int> ChangeOrganizationStatus(Guid id);
        Task<List<Organization>> GetTop5Organization();
        Task<Organization?> GetOrganization(Expression<Func<Organization, bool>> filter);
        
        // 2024-10-16
        Task<Organization?> GetOrganizationInformation(Expression<Func<Organization, bool>> filter);
        Task<int> MemberJoinedOrganization(Guid id);

        // 2024-9-30
        // Recent item
        Task<List<Request>> ViewRecentItem();

        // 2024-10-1
        // Count (For Dashboard)
        Task<int> CountUser();
        Task<int> CountOrganization();
        Task<int> CountRequest();
        Task<int> CountProject();

        // 2024-10-2
        // Project
        Task<List<Project>> ViewProjects();
        Task<Project?> GetProjects(Expression<Func<Project, bool>> filter);
        Task<bool> BanProject(Guid id);

        // 2024-28-10
        Task<Project?> GetProjectInfo(Expression<Func<Project, bool>> filter);


        // 2024-10-4
        // Report
        Task<List<Report>> ViewReport();

        // 2024-10-27
        Task<List<UserToProjectTransactionHistory>> ViewUserToProjectTransactionInHistory(Expression<Func<UserToProjectTransactionHistory, bool>> filter);

        Task<List<ProjectResource>> ViewUserToProjectResource(Expression<Func<ProjectResource, bool>> filter);

        // !Withdraw
        // *2024-6-11
        Task<List<Withdraw>> ReviewWithdraw(Expression<Func<Withdraw, bool>> filer);

        Task<int> CountProjectReport(string type, Guid id);


    }
}