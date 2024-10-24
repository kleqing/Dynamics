using AutoMapper;
using Dynamics.DataAccess;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models;
using Dynamics.Models.Models.Dto;
using Dynamics.Models.Models.ViewModel;
using Dynamics.Utility;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;
using Dynamics.Models.Models.DTO;
using Project = Dynamics.Models.Models.Project;

namespace Dynamics.Services;

public class ProjectService : IProjectService
{
    private readonly IMapper _mapper;
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectResourceRepository _projectResourceRepo;
    private readonly IProjectMemberRepository _projectMemberRepo;
    private readonly IOrganizationRepository _organizationRepo;
    private readonly IUserToProjectTransactionHistoryRepository _userToProjectTransactionHistoryRepo;
    private readonly IOrganizationToProjectTransactionHistoryRepository _organizationToProjectTransactionHistoryRepo;
    private readonly IProjectHistoryRepository _projectHistoryRepo;
    private readonly CloudinaryUploader _cloudinaryUploader;
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _accessor;
    private readonly IRequestRepository _requestRepo;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(IMapper mapper, IProjectRepository projectRepo,
        IProjectResourceRepository projectResourceRepo,
        IProjectMemberRepository projectMemberRepo, ApplicationDbContext context,
        IHttpContextAccessor Accessor,
        IRequestRepository requestRepository,
        IOrganizationRepository organizationRepository,
        IUserToProjectTransactionHistoryRepository userToProjectTransactionHistoryRepository,
        IOrganizationToProjectTransactionHistoryRepository organizationToProjectTransactionHistoryRepository,
        IProjectHistoryRepository projectHistoryRepository,
        CloudinaryUploader cloudinaryUploader,
        ILogger<ProjectService> logger)
    {
        _logger = logger;
        _mapper = mapper;
        _projectRepo = projectRepo;
        _projectResourceRepo = projectResourceRepo;
        _projectMemberRepo = projectMemberRepo;
        _context = context;
        _accessor = Accessor;
        _requestRepo = requestRepository;
        _organizationRepo = organizationRepository;
        _userToProjectTransactionHistoryRepo = userToProjectTransactionHistoryRepository;
        _organizationToProjectTransactionHistoryRepo = organizationToProjectTransactionHistoryRepository;
        _projectHistoryRepo = projectHistoryRepository;
        _cloudinaryUploader = cloudinaryUploader;
    }


    public ProjectOverviewDto MapToProjectOverviewDto(Project p)
    {
        if (p.ProjectMember.IsNullOrEmpty()) throw new Exception("WARNING PROJECT MEMBER IS EMPTY");
        var tempProjectOverviewDto = _mapper.Map<ProjectOverviewDto>(p);
        // Get leader project
        var leader = p.ProjectMember.FirstOrDefault(pm => pm.ProjectID == p.ProjectID && pm.Status == 3);
        if (leader == null)
        {
            leader = p.ProjectMember.FirstOrDefault(pm => pm.ProjectID == p.ProjectID && pm.Status == 2);
        }

        if (leader == null) throw new Exception("No leader for project found");
        tempProjectOverviewDto.ProjectLeader = leader.User;
        tempProjectOverviewDto.ProjectMembers = p.ProjectMember.Count(pm => pm.ProjectID == p.ProjectID);
        tempProjectOverviewDto.ProjectProgress = GetProjectProgress(p);
        var moneyRaised = p.ProjectResource.FirstOrDefault(pr =>
            pr.ResourceName.Equals("Money", StringComparison.CurrentCultureIgnoreCase) && pr.ProjectID == p.ProjectID);
        if (moneyRaised != null)
        {
            tempProjectOverviewDto.ProjectRaisedMoney = moneyRaised.Quantity ?? 0;
        }

        tempProjectOverviewDto.Organization = p.Organization;
        if (p.ProjectAddress != null)
        {
            var location = p.ProjectAddress.Split(",");
            var city = location[0];
            if (location.Length == 4)
            {
                city = location[3];
            }

            tempProjectOverviewDto.ProjectAddress = city;
        }

        return tempProjectOverviewDto;
    }

    public List<ProjectOverviewDto> MapToListProjectOverviewDto(List<Project> projects)
    {
        var resultDtos = new List<ProjectOverviewDto>();
        foreach (var p in projects)
        {
            if (p.ProjectMember.IsNullOrEmpty()) throw new Exception("WARNING PROJECT MEMBER IS EMPTY");
            if (p.ProjectResource.IsNullOrEmpty()) throw new Exception("WARNING PROJECT RESOURCE IS EMPTY");
            var tempProjectOverviewDto = _mapper.Map<ProjectOverviewDto>(p);
            // Get leader(s) project
            var leaders = p.ProjectMember.Where(pm => pm.ProjectID == p.ProjectID && pm.Status >= 2);
            // If project has a leader with status = 3 => That person is leader
            // If project has no leader, but a CEO (Status = 2) => That person will be leader
            ProjectMember leader = null;
            foreach (var l in leaders)
            {
                if (l.Status == 3)
                {
                    leader = l;
                    break;
                }
                if (l.Status == 2)
                {
                    leader = l;
                    break;
                }
                
            }
            if (leader == null) throw new Exception("No leader for project found for project: " + p.ProjectName);
            tempProjectOverviewDto.ProjectLeader = leader.User;
            tempProjectOverviewDto.ProjectMembers = p.ProjectMember.Count(pm => pm.ProjectID == p.ProjectID);
            tempProjectOverviewDto.ProjectProgress = GetProjectProgress(p);
            var moneyRaised = p.ProjectResource.FirstOrDefault(pr =>
                pr.ResourceName.Equals("Money", StringComparison.CurrentCultureIgnoreCase) &&
                pr.ProjectID == p.ProjectID);
            if (moneyRaised != null)
            {
                tempProjectOverviewDto.ProjectRaisedMoney = moneyRaised.Quantity ?? 0;
            }

            // Get project province address
            if (!string.IsNullOrEmpty(p.ProjectAddress))
            {
                var location = p.ProjectAddress.Split(",");
                var city = location[0];
                if (location.Length == 4)
                {
                    city = location[3];
                }

                tempProjectOverviewDto.ProjectAddress = city;
            }
            else
            {
                tempProjectOverviewDto.ProjectAddress = p.ProjectAddress;
            }

            // Get project first attachment
            if (p.Attachment != null) tempProjectOverviewDto.Attachment = p.Attachment.Split(",").FirstOrDefault();
            resultDtos.Add(tempProjectOverviewDto);
        }

        return resultDtos;
    }

    /**
     * Make sure the param p already included the project resource
     */
    public int? GetProjectProgress(Project p)
    {
        if (p.ProjectResource == null)
            throw new Exception("PLEASE INCLUDE THE PROJECT RESOURCE IN PROJECT ENTITY BEFORE USING THIS FUNCTION");
        var resourceNumbers = p.ProjectResource.Where(pr => pr.ResourceName.ToLower().Equals("money"))
            .Select(resource => new
            {
                quantity = resource.Quantity,
                expectedQuantity = resource.ExpectedQuantity
            }).FirstOrDefault();
        if (resourceNumbers == null) return -1;
        if (resourceNumbers.expectedQuantity == 0) return 0;
        var progress = resourceNumbers.quantity * 100 / resourceNumbers.expectedQuantity;
        return progress <= 100 ? progress : 100; // If project donation exceeded the expected, only display 100
    }

    //code convert controller to service-----------------------------------------------------------------------------------------
    //get statistic of a project
    public async Task<List<string>> GetStatisticOfProjectAsync(Guid projectID)
    {
        _logger.LogWarning("GetStatisticOfProjectAsync is here");
        var projectObj = _projectRepo.GetProjectAsync(x => x.ProjectID.Equals(projectID)).Result;
        if (projectObj != null)
        {
            var projectResouceMoney =
                projectObj.ProjectResource.FirstOrDefault(x =>
                    x.ResourceName.ToString().ToLower().Trim().Equals("money"));
            if (projectResouceMoney != null)
            {
                var progressValue = (double)projectResouceMoney.Quantity / projectResouceMoney.ExpectedQuantity *
                                    100;
                //cal nums contributor
                var numberOfProjectContributor = _context.UserToProjectTransactionHistories
                    .Include(x => x.ProjectResource)
                    .Where(x => x.ProjectResource.ProjectID.Equals(projectID) && x.Status == 1)
                    .Select(x => x.UserID)
                    .Distinct().Count();
                numberOfProjectContributor += _context.OrganizationToProjectTransactionHistory
                    .Include(x => x.ProjectResource).Include(x => x.OrganizationResource)
                    .Where(x => x.ProjectResource.ProjectID.Equals(projectID) && x.Status == 1)
                    .Select(x => x.OrganizationResource.OrganizationID)
                    .Distinct().Count();
                var timeLeft = projectObj.EndTime.HasValue
                    ? projectObj.EndTime.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Now
                    : TimeSpan.Zero;
                if (progressValue.ToString() == null || numberOfProjectContributor.ToString() == null ||
                    timeLeft.ToString() == null || projectResouceMoney.ToString() == null)
                {
                    return null;
                }

                List<string> statistic = new List<string>()
                {
                    projectResouceMoney?.Quantity.ToString(), projectResouceMoney?.ExpectedQuantity.ToString(),
                    progressValue.ToString(), numberOfProjectContributor.ToString(), timeLeft.ToString()
                };
                return statistic;
            }
        }

        return null;
    }

    //return list my project
    public async Task<MyProjectVM> ReturnMyProjectVMAsync(Guid userID)
    {
        //get all project
        var allProjects = await _projectRepo.GetAllProjectsAsync();
        //get project that user has joined
        var projectMemberList =
            _projectMemberRepo.FilterProjectMember(
                x => x.UserID.Equals(userID) && x.Status >= 1 && x.Project.ProjectStatus >= 0);
        List<ProjectOverviewDto> projectsIAmMember = new List<ProjectOverviewDto>();
        List<ProjectOverviewDto> projectsILead = new List<ProjectOverviewDto>();
        HashSet<Guid> userProjectIds = new HashSet<Guid>();

        foreach (var projectMember in projectMemberList)
        {
            var project = await _projectRepo.GetProjectAsync(x => x.ProjectID.Equals(projectMember.ProjectID));
            if (project != null)
            {
                var ProjectMemberOfUser = _projectMemberRepo.FilterProjectMember(p =>
                    p.ProjectID.Equals(projectMember.ProjectID) && p.UserID.Equals(userID));
                var statusProjectMemberOfUser = ProjectMemberOfUser?.FirstOrDefault()?.Status;
                var dto = MapToProjectOverviewDto(project);
                userProjectIds.Add(project.ProjectID);
                if (statusProjectMemberOfUser >= 2)
                {
                    //get project that user join as a leader
                    projectsILead.Add(dto);
                }
                else
                {
                    //get project that user join as a member
                    projectsIAmMember.Add(dto);
                }
            }
        }

        return new MyProjectVM()
        {
            ProjectsILead = projectsILead,
            ProjectsIAmMember = projectsIAmMember
        };
    }

    public async Task<AllProjectsVM> ReturnAllProjectsVMsAsync()
    {
        var allProjects = await _projectRepo.GetAllProjectsAsync();
        var allActiveProjectsDto = new List<ProjectOverviewDto>();
        foreach (var p in allProjects)
        {
            var dto = MapToProjectOverviewDto(p);
            if (dto.ProjectStatus >= 0)
            {
                allActiveProjectsDto.Add(dto);
            }
        }

        var result = new AllProjectsVM
        {
            allActiveProjects = allActiveProjectsDto
        };
        return result;
    }
    public async Task<DetailProjectVM> ReturnDetailProjectVMAsync(Guid projectID, HttpContext context)
    {
        var projectObj =
            await _projectRepo.GetProjectAsync(p => p.ProjectID.Equals(projectID));
        if (projectObj != null)
        {
            // TODO
            var request = await _requestRepo.GetAsync(r => r.RequestID.Equals(projectObj.RequestID));
            if (request != null)
            {
                projectObj.Request = request;
            }
            context.Session.SetString("currentProjectID", projectObj.ProjectID.ToString());
            var leaderOfProject = await GetProjectLeaderAsync(projectObj.ProjectID);
            context.Session.SetString("currentProjectLeaderID", leaderOfProject.UserID.ToString());
            var ceoOfProject = FilterMemberOfProject(x => x.Status == 2 && x.ProjectID == projectObj.ProjectID);
            context.Session.SetString("currentProjectCEOID", ceoOfProject[0].UserID.ToString());

            List<string> statistic = await GetStatisticOfProjectAsync(projectObj.ProjectID);
            DetailProjectVM detailProjectVM = new DetailProjectVM()
            {
                CurrentProject = projectObj,
                CurrentLeaderProject = leaderOfProject,
                CurrentAmountOfMoneyDonate = Convert.ToInt32(statistic[0]),
                ExpectedAmountOfMoneyDonate = Convert.ToInt32(statistic[1]),
                ProgressDonate = Convert.ToDouble(statistic[2]),
                NumberOfProjectContributor = Convert.ToInt32(statistic[3]),
                TimeLeftEndDay = Convert.ToInt32(statistic[3]),
                Random5Donors = await GetRandom5DonorsAsync(projectObj.ProjectID)
            };
            if (detailProjectVM != null)
            {
                return detailProjectVM;
            }
        }
        return null;
    }
    //get images
    public async Task<string> GetAllImagesAsync(Guid id, string owner)
    {
        var resImgPath = "";
        if (!string.IsNullOrEmpty(owner) && owner.Equals("Project"))
        {
            var projectObj = await _context.Projects.FirstOrDefaultAsync(x => x.ProjectID.Equals(id));
            if (projectObj != null)
            {
                resImgPath = projectObj.Attachment;
            }
        }
        else if (!string.IsNullOrEmpty(owner) && owner.Equals("Phase"))
        {
            var historyObj = await _context.Histories.FirstOrDefaultAsync(x => x.HistoryID.Equals(id));
            if (historyObj != null)
            {
                resImgPath = historyObj.Attachment;
            }
        }

        return resImgPath;
    }

    public async Task<bool> DeleteImageAsync(string imgPath, Guid phaseID)
    {
        if(string.IsNullOrEmpty(imgPath) || phaseID == Guid.Empty) return false;
        if (phaseID != Guid.Empty)
        {
            var allImagesOfPhase = await GetAllImagesAsync(phaseID, "Phase");
            var historyObj =
                await _projectHistoryRepo.GetAllPhaseReportsAsync(x => x.HistoryID.Equals(phaseID));
            if (historyObj != null && allImagesOfPhase != null)
            {
                if (allImagesOfPhase.Length > 0)
                {
                    foreach (var img in allImagesOfPhase.Split(','))
                    {
                        if (img.Equals(imgPath))
                        {
                            allImagesOfPhase = allImagesOfPhase.Split(',').Count() == 1? allImagesOfPhase.Replace(img, ""): allImagesOfPhase.Replace(img + ",","");
                        }
                    }

                    historyObj[0].Attachment = allImagesOfPhase;
                }

                var res = await _projectHistoryRepo.EditPhaseReportAsync(historyObj[0]);
                if (res) return true;
            }
        }
        else
        {
            var currentProjectID = _accessor.HttpContext.Session.GetString("currentProjectID");
            var allImagesOfProject =
                await GetAllImagesAsync(new Guid(currentProjectID), "Project");
            var projectObj =
                await _projectRepo.GetProjectAsync(x => x.ProjectID.Equals(new Guid(currentProjectID)));
            if (projectObj != null && allImagesOfProject != null)
            {
                if (allImagesOfProject.Length > 0)
                {
                    foreach (var img in allImagesOfProject.Split(','))
                    {
                        if (img.Equals(imgPath))
                        {
                            allImagesOfProject = allImagesOfProject.Split(',').Count() == 1 ? allImagesOfProject.Replace(img, "") : allImagesOfProject.Replace(img + ",", "");
                        }
                    }

                    projectObj.Attachment = allImagesOfProject;
                }

                var res = await _projectRepo.UpdateAsync(projectObj);
                if (res) return true;
            }
        }

        return false;
    }

    public async Task<string> UploadImagesAsync(List<IFormFile> images, string folder)
    {
        if (images != null && images.Count() > 0)
        {
            var resAttachment = await Util.UploadImages(images, $@"{folder}");
            return resAttachment;
        }

        return string.Empty;
    }

    public async Task<string> UpdateProjectProfileAsync(UpdateProjectProfileRequestDto updateProject,
        List<IFormFile> images)
    {
        _logger.LogWarning("update 1 here");
        var projectObj = _mapper.Map<Dynamics.Models.Models.Project>(updateProject);
        if (projectObj != null)
        {
            //var resImage = await UploadImagesAsync(images, @"images\Project");
            var resImage = await _cloudinaryUploader.UploadMultiImagesAsync(images);
            if (resImage.Equals("Wrong extension")) return resImage;
            projectObj.Attachment = !resImage.Equals("No file")? resImage : projectObj.Attachment;
            var res = await UpdateProjectAlongWithUpdateLeaderAsync(projectObj, updateProject.NewLeaderID);
            if (!res) return MyConstants.Error;
            return MyConstants.Success;
        }

        return MyConstants.Error;
    }
    public async Task<bool> UpdateProjectAlongWithUpdateLeaderAsync(Project entity, Guid newProjectLeaderID)
    {
        _logger.LogWarning("update project is here");
        var res = await _projectRepo.UpdateAsync(entity);
        if (!res) return false;
        //updating 2 member who is new and old leader of project
        var oldProjectLeaderUser = _accessor.HttpContext.Session.GetString("currentProjectLeaderID");
        _logger.LogWarning("old leader is here");
        var oldProjectLeader = _projectMemberRepo.FilterProjectMember(x => x.UserID.Equals(new Guid(oldProjectLeaderUser)) && x.ProjectID.Equals(entity.ProjectID)).FirstOrDefault();
        _logger.LogWarning("new leader is here");
        var newProjectLeader = await _context.ProjectMembers.FirstOrDefaultAsync(x =>
            x.UserID.Equals(newProjectLeaderID) && x.ProjectID.Equals(entity.ProjectID));
        var ceoOfProjectID = _accessor.HttpContext.Session.GetString("currentProjectCEOID");
        if (oldProjectLeader != null && newProjectLeader != null &&
            !oldProjectLeader.UserID.Equals(newProjectLeader.UserID))
        {
            if (oldProjectLeader.UserID.ToString().Equals(ceoOfProjectID))
            {
                oldProjectLeader.Status = 2;
            }
            else
            {
                oldProjectLeader.Status = 1;
            }

            _logger.LogWarning("update old leader is here");
            await _projectMemberRepo.UpdateAsync(oldProjectLeader);
            if (newProjectLeader.UserID.ToString().Equals(ceoOfProjectID))
            {
                newProjectLeader.Status = 2;
            }
            else
            {
                newProjectLeader.Status = 3;
            }
            _logger.LogWarning("update new leader is here");
            await _projectMemberRepo.UpdateAsync(newProjectLeader);
            return true;
        }
        return true;
    }

    //working with member of project-----------------------------------------------------------------------------------------
    //get leader of project
    public async Task<User> GetProjectLeaderAsync(Guid projectID)
    {
        _logger.LogWarning("Project leader is here");
        var projectObj = await _projectRepo.GetProjectAsync(x => x.ProjectID.Equals(projectID));
        _logger.LogWarning("leader is here");
        ProjectMember leaderProjectMembers = projectObj?.ProjectMember.Where(x => x.Status == 3).FirstOrDefault();
        //if no leader then leader is the ceo of organization
        if (leaderProjectMembers == null)
        {
            _logger.LogWarning("ceo is here");
            leaderProjectMembers = projectObj?.ProjectMember.Where(x => x.Status == 2).FirstOrDefault();
        }

        if (leaderProjectMembers != null)
        {
            return leaderProjectMembers?.User;
        }

        return null;
    }

    //filter users of ProjectMember table
    public List<User> FilterMemberOfProject(Expression<Func<ProjectMember, bool>> filter)
    {
        IQueryable<ProjectMember> projectMemberList = _context.ProjectMembers
            .Include(x => x.User)
            .Include(x => x.Project).Where(filter);
        List<User> members = new List<User>();
        if (members != null)
        {
            foreach (var u in projectMemberList)
            {
                members.Add(u.User);
            }

            return members;
        }

        return null;
    }

    public async Task<string> SendJoinProjectRequestAsync(Guid projectID, Guid memberID)
    {
        var existingJoinRequest = _projectMemberRepo
            .FilterProjectMember(p => p.ProjectID.Equals(projectID) && p.UserID.Equals(memberID)).FirstOrDefault();
        if (existingJoinRequest == null) // Not existed, add new with status = 0
        {
            var res = await _projectMemberRepo.AddJoinRequest(new ProjectMember()
            {
                UserID = memberID,
                ProjectID = projectID,
                Status = 0
            });
            if (res)
            {
                return MyConstants.Success;
            }

            return MyConstants.Error;
        }
        if (existingJoinRequest.Status == -1) // Denied, update from -1 -> 0
        {
            existingJoinRequest.Status = 0;
            var res = await _projectMemberRepo.UpdateAsync(existingJoinRequest);
            if (res) return MyConstants.Success;
        }
        // Pending: 0, only warning
        return MyConstants.Warning;
    }

    public async Task<bool> AcceptJoinProjectRequestAllAsync(Guid projectID)
    {
        var allJoinRequest =
            _projectMemberRepo.FilterProjectMember(
                p => p.ProjectID.Equals(projectID) && p.Status == 0);
        if (allJoinRequest == null)
        {
            return false;
        }

        foreach (var joinRequest in allJoinRequest)
        {
            var res = await _projectMemberRepo.AcceptJoinRequestAsync(joinRequest.UserID,
                projectID);
            if (!res)
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> DenyJoinProjectRequestAllAsync(Guid projectID)
    {
        var allJoinRequest =
            _projectMemberRepo.FilterProjectMember(
                p => p.ProjectID.Equals(projectID) && p.Status == 0);
        if (allJoinRequest == null)
        {
            return false;
        }

        foreach (var joinRequest in allJoinRequest)
        {
            var res = await _projectMemberRepo.DenyJoinRequestAsync(joinRequest.UserID,
                projectID);
            if (!res)
            {
                return false;
            }
        }

        return true;
    }


    //get transaction information of project
    public async Task<List<UserToProjectTransactionHistory>> GetRandom5DonorsAsync(Guid projectID)
    {
        _logger.LogWarning("GetRandom5DonorsAsync is here");
        var userDonate = await _context.UserToProjectTransactionHistories
            .Include(x => x.User).Include(x => x.ProjectResource)
            .Where(x => x.ProjectResource.ProjectID.Equals(projectID) && x.Status == 1)
            .OrderBy(x => Guid.NewGuid())
            .Take(5)
            .ToListAsync();
        return userDonate;
    }

    public async Task<SendDonateRequestVM> ReturnSendDonateRequestVMAsync(Guid projectID, string donor)
    {
        if (!string.IsNullOrEmpty(donor) && donor.Equals("User"))
        {
            var currentUserID = _accessor.HttpContext.Session.GetString("currentUserID");
            List<UserToProjectTransactionHistory> donateHistoryOfUser;
            if (currentUserID != null)
            {
                donateHistoryOfUser = await _userToProjectTransactionHistoryRepo.GetAllUserDonateAsync(
                    u => u.UserID.Equals(new Guid(currentUserID)) && u.ProjectResource.ProjectID.Equals(projectID)
                );
                return new SendDonateRequestVM()
                {
                    ProjectID = projectID,
                    TypeDonor = donor,
                    UserTransactionHistory = donateHistoryOfUser
                };
            }
        }

        return null;
    }

    public async Task<string> SendDonateRequestAsync(SendDonateRequestVM sendDonateRequestVM, List<IFormFile> images)
    {
        if (sendDonateRequestVM != null)
        {
            var projectResourceObj = await _projectResourceRepo.GetAsync(x =>
                x.ResourceID.Equals(sendDonateRequestVM.UserDonate.ProjectResourceID));
            var quantityAfterDonate = sendDonateRequestVM.UserDonate.Amount + projectResourceObj.Quantity;
            if (quantityAfterDonate > projectResourceObj.ExpectedQuantity)
            {
                return "Exceed";
            }

            if (!string.IsNullOrEmpty(sendDonateRequestVM.TypeDonor) &&
                sendDonateRequestVM.TypeDonor.Equals("User"))
            {
                if (sendDonateRequestVM.UserDonate.UserID != Guid.Empty)
                {
                    var resImage = await _cloudinaryUploader.UploadMultiImagesAsync(images);
                    if (resImage.Equals("Wrong extension") || resImage.Equals("No file"))
                        return resImage;
                    else sendDonateRequestVM.UserDonate.Attachments = resImage;
                    var res = await _userToProjectTransactionHistoryRepo.AddUserDonateRequestAsync(sendDonateRequestVM
                        .UserDonate);
                    if (!res)
                    {
                        return MyConstants.Error;
                    }

                    return MyConstants.Success;
                }
            }
        }

        return MyConstants.Error;
    }

    public async Task<ProjectTransactionHistoryVM> ReturnProjectTransactionHistoryVMAsync(Guid projectID)
    {
        var allUserDonate =
            await _userToProjectTransactionHistoryRepo.GetAllUserDonateAsync(u =>
                u.ProjectResource.ProjectID.Equals(projectID) && u.Status == 1 || u.Status == -1);
        if (allUserDonate == null)
        {
            return null;
        }

        var allOrganizationDonate =
            await _organizationToProjectTransactionHistoryRepo.GetAllOrganizationDonateAsync(u =>
                u.ProjectResource.ProjectID.Equals(projectID) && u.Status == 1 || u.Status == -1);
        if (allOrganizationDonate == null)
        {
            return null;
        }

        return new ProjectTransactionHistoryVM()
        {
            UserDonate = allUserDonate,
            OrganizationDonate = allOrganizationDonate
        };
    }

    public async Task<bool> AcceptDonateProjectRequestAllAsync(Guid projectID, string donor,
        List<IFormFile> proofImages)
    {
        var resImage = await _cloudinaryUploader.UploadMultiImagesAsync(proofImages);
        if (resImage.Equals("Wrong extension") || resImage.Equals("No file"))
            return false;
        else
        {
            switch (donor)
            {
                case "User":
                    var allUserDonateRequest =
                        await _userToProjectTransactionHistoryRepo.GetAllUserDonateAsync(
                            u => u.ProjectResource.ProjectID.Equals(projectID) && u.Status == 0);
                    if (allUserDonateRequest == null)
                    {
                        return false;
                    }

                    foreach (var userDonate in allUserDonateRequest)
                    {
                        userDonate.Attachments = resImage;
                        var res = await _userToProjectTransactionHistoryRepo.AcceptUserDonateRequestAsync(userDonate);
                        if (!res)
                        {
                            return false;
                        }
                    }

                    break;
                case "Organization":
                    var allOrgDonateRequest =
                        await _organizationToProjectTransactionHistoryRepo.GetAllOrganizationDonateAsync(
                            u => u.ProjectResource.ProjectID.Equals(projectID) && u.Status == 0);
                    if (allOrgDonateRequest == null)
                    {
                        return false;
                    }

                    foreach (var orgDonate in allOrgDonateRequest)
                    {
                        orgDonate.Attachments = resImage;
                        var res =
                            await _organizationToProjectTransactionHistoryRepo.AcceptOrgDonateRequestAsync(orgDonate);
                        if (!res)
                        {
                            return false;
                        }
                    }

                    break;
            }
        }

        return true;
    }

    public async Task<bool> DenyDonateProjectRequestAllAsync(Guid projectID, string donor, string reasonToDeny)
    {
        switch (donor)
        {
            case "User":
                var allUserDonateRequest =
                    await _userToProjectTransactionHistoryRepo.GetAllUserDonateAsync(
                        u => u.ProjectResource.ProjectID.Equals(projectID) && u.Status == 0);
                if (allUserDonateRequest == null)
                {
                    return false;
                }

                foreach (var userDonate in allUserDonateRequest)
                {
                    userDonate.Message += "\nReason: " + reasonToDeny;
                    var res = await _userToProjectTransactionHistoryRepo.DenyUserDonateRequestAsync(userDonate);
                    if (!res)
                    {
                        return false;
                    }
                }

                break;
            case "Organization":
                var allOrgDonateRequest =
                    await _organizationToProjectTransactionHistoryRepo.GetAllOrganizationDonateAsync(
                        u => u.ProjectResource.ProjectID.Equals(projectID) && u.Status == 0);
                if (allOrgDonateRequest == null)
                {
                    return false;
                }

                foreach (var orgDonate in allOrgDonateRequest)
                {
                    orgDonate.Message += "\nReason: " + reasonToDeny;
                    var res = await _organizationToProjectTransactionHistoryRepo.DenyOrgDonateRequestAsync(orgDonate);
                    if (!res)
                    {
                        return false;
                    }
                }

                break;
        }

        return true;
    }

    //manage resource of project-----------------------------------------------------------------------------------------
    public async Task<string> UpdateProjectResourceTypeAsync(ProjectResource projectResource)
    {
        var projectResourceObj =
            await _projectResourceRepo.FilterProjectResourceAsync(p =>
                p.ResourceID.Equals(projectResource.ResourceID));

        if (projectResourceObj.FirstOrDefault() != null)
        {
            //check whether the resource has same name and same unit is existed
            var existingResource = await _projectResourceRepo.FilterProjectResourceAsync(p =>
                p.ResourceID != projectResource.ResourceID &&
                p.ProjectID.Equals(projectResource.ProjectID) &&
                p.ResourceName.Equals(projectResource.ResourceName) && p.Unit.Equals(projectResource.Unit));
            if (existingResource.FirstOrDefault() != null)
            {
                return "Existed";
            }

            var res = await _projectResourceRepo.UpdateResourceTypeAsync(projectResource);
            if (res)
            {
                return MyConstants.Success;
            }
        }

        return MyConstants.Error;
    }

    //add constraint for project update
    public async Task<List<DateTime>> GetExistingReportDatesAsync(Guid projectID)
    {
        // Retrieve the list of dates where a report exists for the given project
        return await _context.Histories
            .Where(x => x.ProjectID == projectID)
            .Select(x => x.Date.ToDateTime(TimeOnly.MinValue)) // Adjust depending on your column type
            .ToListAsync();
    }

    public async Task<DateTime> GetReportDateAsync(Guid projectID, Guid historyID)
    {
        var history =
            await _context.Histories.FirstOrDefaultAsync(x =>
                x.ProjectID.Equals(projectID) && x.HistoryID.Equals(historyID));
        if (history != null)
        {
            return history.Date.ToDateTime(TimeOnly.MinValue);
        }

        return DateTime.MinValue;
    }

    public async Task<string> AddProjectPhaseReportAsync(History history, List<IFormFile> images)
    {
        if (history == null) return MyConstants.Error;
        history.HistoryID = Guid.NewGuid();

        if (images != null && images.Count() > 0)
        {
            //var resImage = await UploadImagesAsync(images, @"images\Project");
            var resImage = await _cloudinaryUploader.UploadMultiImagesAsync(images);
            if (resImage.Equals("No file") || resImage.Equals("Wrong extension"))
            {
                return resImage;
            }

            history.Attachment = resImage;
        }

        var res = await _projectHistoryRepo.AddPhaseReportAsync(history);
        if (!res) return MyConstants.Error;
        return MyConstants.Success;
    }

    public async Task<string> EditProjectPhaseReportAsync(History history, List<IFormFile> images)
    {
        if (history == null) return MyConstants.Error;
        if (images != null && images.Count() > 0)
        {
            //var resImage = await UploadImagesAsync(images, @"images\Project");
            var resImage = await _cloudinaryUploader.UploadMultiImagesAsync(images);
            if (resImage.Equals("No file") || resImage.Equals("Wrong extension"))
            {
                return resImage;
            }

            history.Attachment = resImage;
        }

        var res = await _projectHistoryRepo.EditPhaseReportAsync(history);
        if (!res) return MyConstants.Error;
        return MyConstants.Success;
    }

    public async Task<List<Project>> GetProjectsWithExpressionAsync(Expression<Func<Project, bool>> filter = null)
    {
        if (filter == null)
        {
            return await _projectRepo.GetAllQueryable()
                .Include(p => p.ProjectMember)
                .Include(p => p.ProjectResource).ToListAsync();
        }

        return await _projectRepo.GetAllQueryable()
            .Include(p => p.ProjectMember)
            .Include(p => p.ProjectResource)
            .Where(filter).ToListAsync();
    }
}