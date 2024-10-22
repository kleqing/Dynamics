using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;
using Dynamics.Services;
using Dynamics.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Web.Mvc;
using CloudinaryDotNet.Actions;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace Dynamics.Controllers
{
    public class RequestController : Controller
    {
        private readonly IRequestRepository _requestRepo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RequestController> _logger;
        private readonly IRequestService _requestService;
        private readonly CloudinaryUploader _cloudinaryUploader;

        public RequestController(IRequestRepository requestRepository, UserManager<IdentityUser> userManager,
            ILogger<RequestController> logger, IRequestService requestService, CloudinaryUploader cloudinaryUploader)
        {
            _requestRepo = requestRepository;
            _userManager = userManager;
            _logger = logger;
            _requestService = requestService;
            _cloudinaryUploader = cloudinaryUploader;
        }

        // View all requests
        public async Task<IActionResult> Index(string searchQuery, string filterQuery = "All", int pageNumber = 1,
            int pageSize = 12)
        {
            // Only get the one that is accepted by admin (status = 1)
            var requests = _requestRepo.GetAllQueryable(r => r.Status == 1);
            // search
            if (!string.IsNullOrEmpty(searchQuery))
            {
                requests = await _requestRepo.SearchIndexFilterAsync(searchQuery, filterQuery);
            }

            // if (!dateFrom.ToString("yyyy-MM-dd").Equals("0001-01-01"))
            // {
            //     requests = await _requestRepo.GetRequestDateFilterAsync(requests, dateFrom, dateTo);
            // } 

            // pagination
            var totalRequest = await requests.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRequest / pageSize);
            var paginatedRequests = await _requestRepo.PaginateAsync(requests, pageNumber, pageSize);

            ViewBag.currentPage = pageNumber;
            ViewBag.totalPages = totalPages;

            var requestOverviewDto = _requestService.MapToListRequestOverviewDto(paginatedRequests);
            return View(requestOverviewDto);
        }

        [Authorize]
        public async Task<IActionResult> MyRequest(string searchQuery, string filterQuery = "All", int pageNumber = 1,
            int pageSize = 12)
        {
            User currentUser = HttpContext.Session.GetCurrentUser();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var requests = _requestRepo.GetAllById(currentUser.UserID);
            // search
            if (!string.IsNullOrEmpty(searchQuery))
            {
                requests = _requestRepo.SearchIdFilterAsync(searchQuery, filterQuery, currentUser.UserID);
            }

            //date filter
            // if (!dateFrom.ToString("yyyy-MM-dd").Equals("0001-01-01"))
            // {
            //     requests = await _requestRepo.GetRequestDateFilterAsync(requests, dateFrom, dateTo);
            // } 
            //pagination
            var totalRequest = await requests.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRequest / pageSize);
            var paginatedRequests = await _requestRepo.PaginateAsync(requests, pageNumber, pageSize);

            ViewBag.currentPage = pageNumber;
            ViewBag.totalPages = totalPages;

            // Convert to view models
            var requestOverviewDto = _requestService.MapToListRequestOverviewDto(paginatedRequests);
            return View(requestOverviewDto);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var request = await _requestRepo.GetAsync(r => r.RequestID.Equals(id));
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        public IActionResult Create()
        {
            var userJson = HttpContext.Session.GetString("user");
            var user = JsonConvert.DeserializeObject<User>(userJson);

            var viewModel = new RequestCreateVM
            {
                UserEmail = user.UserEmail,
                UserPhoneNumber = user.UserPhoneNumber,
                UserAddress = user.UserAddress,

                RequestTitle = string.Empty,
                Content = string.Empty,
                CreationDate = null,
                RequestEmail = string.Empty,
                RequestPhoneNumber = string.Empty,
                Location = string.Empty,
                isEmergency = 0
            };
            return View(viewModel);
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<IActionResult> Create(Request obj, List<IFormFile> images,
            string? cityNameInput, string? districtNameInput, string? wardNameInput)
        {
            try
            {
                obj.RequestID = Guid.NewGuid();
                /*var date = DateOnly.FromDateTime(DateTime.Now);
                obj.CreationDate = date;*/
                obj.Location += ", " + wardNameInput + ", " + districtNameInput + ", " + cityNameInput;
                var userId = Guid.Empty;
                var userJson = HttpContext.Session.GetString("user");
                if (!string.IsNullOrEmpty(userJson))
                {
                    var user = JsonConvert.DeserializeObject<User>(userJson);
                    userId = user.UserID;
                }

                obj.UserID = userId;
                if (images != null && images.Count > 0)
                {
                    string imagePath = await _cloudinaryUploader.UploadMultiImagesAsync(images);
                    obj.Attachment = imagePath;
                }
                // No Pun
                // else
                // {
                //     obj.Attachment = "/images/Requests/Placeholder/xddFaker.png";
                // }

                _logger.LogInformation("Request created");
                await _requestRepo.AddAsync(obj);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
                TempData[MyConstants.Error] = "Create request failed";
                foreach (var error in ModelState)
                {
                    ModelState.AddModelError(string.Empty, error.Key);
                }

                return RedirectToAction("Create");
            }

            return RedirectToAction("MyRequest", "Request");
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get the currently logged-in user
            _logger.LogInformation("Get logged-in user");
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(user.Id);
            var request = await _requestRepo.GetAsync(r => r.RequestID.Equals(id));
            if (request == null)
            {
                return NotFound();
            }

            if (request.UserID != userId)
            {
                return Forbid(); // If the user is not the owner of the request
            }

            return View(request);
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<IActionResult> Edit(Request obj, List<IFormFile> images,
            string? cityNameInput, string? districtNameInput, string? wardNameInput)
        {
            try
            {
                /*if (!ModelState.IsValid)
                {
                    return View(obj);
                }*/
                // Get the currently logged-in user (role and id)
                obj.Location += ", " + wardNameInput + ", " + districtNameInput + ", " + cityNameInput;
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";
                var userId = Guid.Parse(user.Id);
                // Get existing request
                _logger.LogInformation("Get existing request");
                var existingRequest = await _requestRepo.GetAsync(r => r.RequestID.Equals(obj.RequestID));
                if (existingRequest == null)
                {
                    return NotFound();
                }

                existingRequest.RequestTitle = obj.RequestTitle;
                existingRequest.Content = obj.Content;
                existingRequest.RequestEmail = obj.RequestEmail;
                existingRequest.RequestPhoneNumber = obj.RequestPhoneNumber;
                existingRequest.Location = obj.Location;
                existingRequest.isEmergency = obj.isEmergency;
                if (images != null && images.Count > 0)
                {
                    string imagePath = await _cloudinaryUploader.UploadMultiImagesAsync(images);
                    // append new images if there are existing images
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        existingRequest.Attachment = string.IsNullOrEmpty(existingRequest.Attachment)
                            ? imagePath
                            : existingRequest.Attachment + "," + imagePath;
                    }
                }

                _logger.LogInformation("Edit request");
                await _requestRepo.UpdateAsync(existingRequest);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
                TempData[MyConstants.Error] = "Update request failed";
                foreach (var error in ModelState)
                {
                    ModelState.AddModelError(string.Empty, error.Key);
                }
                return RedirectToAction("Edit");
            }

            return RedirectToAction("MyRequest", "Request");
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Request request = await _requestRepo.GetAsync(r => r.RequestID.Equals(id));
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        [Microsoft.AspNetCore.Mvc.HttpPost, Microsoft.AspNetCore.Mvc.ActionName("Delete")]
        public async Task<IActionResult> DeletePost(Guid? id)
        {
            _logger.LogInformation("Get deleted request");
            Request request = await _requestRepo.GetAsync(r => r.RequestID.Equals(id));
            if (request == null)
            {
                return NotFound();
            }
            _logger.LogInformation("Delete request");
            await _requestRepo.DeleteAsync(request);
            return RedirectToAction("MyRequest", "Request");
        }
    }
}