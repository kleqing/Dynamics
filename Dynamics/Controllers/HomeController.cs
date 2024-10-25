using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models.ViewModel;
using Dynamics.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Dynamics.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IRequestRepository _requestRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IProjectService _projectService;
        private readonly IRequestService _requestService;
        private readonly IOrganizationRepository _organizationRepo;
        private readonly IOrganizationService _organizationService;
        private readonly IPagination _pagination;

        public HomeController(IUserRepository userRepo, IRequestRepository requestRepo,
            IProjectRepository projectRepo, IProjectService projectService, IRequestService requestService,
            IOrganizationRepository organizationRepo, IOrganizationService organizationService, IPagination pagination)
        {
            _userRepo = userRepo;
            _requestRepo = requestRepo;
            _projectRepo = projectRepo;
            _projectService = projectService;
            _requestService = requestService;
            _organizationRepo = organizationRepo;
            _organizationService = organizationService;
            _pagination = pagination;
        }

        // // Landing page
        // public IActionResult Index()
        // {
        //     return View();
        // }

        public async Task<IActionResult> Index()
        {
            // Check if there is an authenticated user, set the session of that user
            if (User.Identity.IsAuthenticated)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var user = _userRepo.GetAsync(u => u.UserEmail == userEmail).Result;
                // Bad user
                if (user == null) return RedirectToAction("Logout", "Auth");
                HttpContext.Session.SetString("user", JsonConvert.SerializeObject(user));
            }

            var requestsQueryable = _requestRepo.GetAllQueryable(r => r.Status == 1); // Get accepted requests
            var paginatedRequest = await _pagination.PaginateAsync(requestsQueryable, 1, 9);
            var requestOverview = _requestService.MapToListRequestOverviewDto(paginatedRequest);

            var projectsQueryable = _projectRepo.GetAllQueryable(p => p.ProjectStatus != -1 && p.ProjectStatus != 2); // Get not banned project and finished
            var projectsPaginated = await _pagination.PaginateAsync(projectsQueryable, 1, 9); // Use the query and apply the pagination
            var projectDtos = _projectService.MapToListProjectOverviewDto(projectsPaginated);

            var successfulProjectsPaginated = await _pagination.PaginateAsync(projectsQueryable.Where(p => p.ProjectStatus == 2), 1, 9); // Apply filter to get successful only
            var successfulProjectDtos = _projectService.MapToListProjectOverviewDto(successfulProjectsPaginated);

            var orgsQueryable = _organizationRepo.GetAll();
            var paginatedOrg = await _pagination.PaginateAsync(orgsQueryable, 1, 9);
            var orgsOverview = _organizationService.MapToOrganizationOverviewDtoList(paginatedOrg);

            var result = new HomepageViewModel
            {
                Requests = requestOverview,
                Projects = projectDtos,
                Organizations = orgsOverview,
                SuccessfulProjects = successfulProjectDtos,
            };
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string? q)
        {
            if (q == null) return RedirectToAction(nameof(Index));
            string[] args = q.Split("-");
            TempData["query"] = q; // Use for display
            var query = q.ToLower();
            // Args < 2 search all
            if (args.Length < 2)
            {
                var requests = _requestRepo.GetAllQueryable();
                // Dynamic so that it can be assigned again by other
                dynamic targets = await requests.Where(r => r.RequestTitle.ToLower().Contains(query)).ToListAsync();
                var requestOverviewDtos = _requestService.MapToListRequestOverviewDto(targets);

                var projects = _projectRepo.GetAllQueryable();
                targets = await projects.Where(r => r.ProjectName.ToLower().Contains(query)).ToListAsync();
                var projectOverviewDtos = _projectService.MapToListProjectOverviewDto(targets);

                var organizations =
                    await _organizationRepo.GetAllOrganizationsWithExpressionAsync();
                targets = organizations.Where(r => r.OrganizationName.ToLower().Contains(query)).ToList();
                var organizationOverviewDtos = _organizationService.MapToOrganizationOverviewDtoList(targets);

                return View(new HomepageViewModel
                {
                    Requests = requestOverviewDtos,
                    Projects = projectOverviewDtos,
                    Organizations = organizationOverviewDtos,
                });
            }
            else
            {
                // Only search by a specific type
                var type = args[0];
                var target = args[1];

                if (type.Contains("req"))
                {
                    var requests = _requestRepo.GetAllQueryable();
                    var targets = requests
                        .Where(r => r.RequestTitle.ToLower().Contains(target)).ToList();
                    var requestOverviewDtos = _requestService.MapToListRequestOverviewDto(targets);
                    return View(new HomepageViewModel
                    {
                        Requests = requestOverviewDtos,
                    });
                }

                if (type.Contains("prj"))
                {
                    var projects = await _projectRepo.GetAllAsync();
                    var targets = projects.Where(r => r.ProjectName.ToLower().Contains(target)).ToList();
                    var projectOverviewDtos = _projectService.MapToListProjectOverviewDto(targets);
                    return View(new HomepageViewModel
                    {
                        Projects = projectOverviewDtos,
                    });
                }

                if (type.Contains("org"))
                {
                    var organizations =
                        await _organizationRepo.GetAllOrganizationsWithExpressionAsync();
                    var targets = organizations
                        .Where(r => r.OrganizationName.ToLower().Contains(target)).ToList();
                    var organizationOverviewDtos = _organizationService.MapToOrganizationOverviewDtoList(targets);
                    return View(new HomepageViewModel
                    {
                        Organizations = organizationOverviewDtos
                    });
                }
            }

            // if we get here invalid search so just back to home
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Announce()
        {
            return View();
        }
    }
}