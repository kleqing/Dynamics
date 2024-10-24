using Dynamics.Models.Models;
using Dynamics.Models.Models.Dto;
using Dynamics.Models.Models.DTO;
using Dynamics.Models.Models.ViewModel;
using System.Linq.Expressions;

namespace Dynamics.Services;

public interface IProjectService
{
    /**
     * Map to dto for display purposes (Card)
     */
    public ProjectOverviewDto MapToProjectOverviewDto(Project p);
    /**
     * Make sure project member and project resource is included!
     */
    List<ProjectOverviewDto> MapToListProjectOverviewDto(List<Project> projects);
    //manage project overview-----------
    Task<List<string>> GetStatisticOfProjectAsync(Guid projectID);
    public Task<MyProjectVM> ReturnMyProjectVMAsync(Guid userID);
    public Task<AllProjectsVM> ReturnAllProjectsVMsAsync();
    Task<bool> UpdateProjectAlongWithUpdateLeaderAsync(Project entity, Guid newProjectLeaderID);
    //manage images of project/phase
    Task<string> GetAllImagesAsync(Guid id, string owner);
    public Task<bool> DeleteImageAsync(string imgPath, Guid phaseID);
    public Task<string> UploadImagesAsync(List<IFormFile> images, string folder);
    public Task<DetailProjectVM> ReturnDetailProjectVMAsync(Guid projectID, HttpContext context);
    public Task<string> UpdateProjectProfileAsync(UpdateProjectProfileRequestDto updateProject, List<IFormFile> images);
    //manage member of project------------
    //using this to get leader type User
    Task<User> GetProjectLeaderAsync(Guid projectID);
    public List<User> FilterMemberOfProject(Expression<Func<ProjectMember, bool>> filter);
    public Task<string> SendJoinProjectRequestAsync(Guid projectID, Guid memberID);
    public Task<bool> AcceptJoinProjectRequestAllAsync(Guid projectID);
    public Task<bool> DenyJoinProjectRequestAllAsync(Guid projectID);
    //manage transaction history of project--------
    public Task<List<UserToProjectTransactionHistory>> GetRandom5DonorsAsync(Guid projectID);
    public Task<SendDonateRequestVM> ReturnSendDonateRequestVMAsync(Guid projectID, string donor);
    Task<string> SendDonateRequestAsync(SendDonateRequestVM sendDonateRequestVM,List<IFormFile> images);
    public Task<ProjectTransactionHistoryVM> ReturnProjectTransactionHistoryVMAsync(Guid projectID);
    Task<bool> AcceptDonateProjectRequestAllAsync(Guid projectID,string donor,List<IFormFile> proofImages);
    Task<bool>DenyDonateProjectRequestAllAsync(Guid projectID,string donor,string reasonToDeny);
    //manage project resource--------------
    Task<string> UpdateProjectResourceTypeAsync(ProjectResource projectResource);
    //manage project phase report---------
    //constraint project phase report
    Task<List<DateTime>> GetExistingReportDatesAsync(Guid projectID);
    Task<DateTime> GetReportDateAsync(Guid projectID, Guid historyID);
    //add and edit project phase report
    Task<string> AddProjectPhaseReportAsync(History history, List<IFormFile> images);
    Task<string> EditProjectPhaseReportAsync(History history, List<IFormFile> images);

    Task<List<Project>> GetProjectsWithExpressionAsync(Expression<Func<Project, bool>> filter = null);
}