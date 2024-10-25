using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models;
using Dynamics.Services;
using Dynamics.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
			ILogger<RequestController> logger,
			IRequestService? requestService, CloudinaryUploader? cloudinaryUploader)
		{
			_requestRepo = requestRepository;
			_userManager = userManager;
			_logger = logger;
			_requestService = requestService;
			_cloudinaryUploader = cloudinaryUploader;
		}
		public async Task<IActionResult> Index(string searchQuery, string filterQuery, 
			DateOnly dateFrom, DateOnly dateTo,
			int pageNumber = 1, int pageSize = 12)
		{
			// Only get the one that is accepted by admin (status = 1)
			var requests = _requestRepo.GetAllQueryable(r => r.Status == 1);
			
			// search
			if (!string.IsNullOrEmpty(searchQuery))
			{
				requests = await _requestRepo.SearchIndexFilterAsync(searchQuery, filterQuery);
			}

			if (!dateFrom.ToString("yyyy-MM-dd").Equals("0001-01-01"))
			{
				requests = await _requestRepo.GetRequestDateFilterAsync(requests, dateFrom, dateTo);
			}
			
			// pagination
			var totalRequest = await requests.CountAsync();
			var totalPages = (int)Math.Ceiling((double)totalRequest / pageSize);
			var paginatedRequests = await _requestRepo.PaginateAsync(requests, pageNumber, pageSize);
			var dtos = _requestService.MapToListRequestOverviewDto(paginatedRequests);
			ViewBag.currentPage = pageNumber;
			ViewBag.totalPages = totalPages;
			return View(dtos);
		}
		
		public async Task<IActionResult> MyRequest(string searchQuery, string filterQuery,
			DateOnly dateFrom, DateOnly dateTo,
			int pageNumber = 1, int pageSize = 12)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Unauthorized();
			}
			Guid userId = Guid.Empty;
			var userJson = HttpContext.Session.GetString("user");
			if (!string.IsNullOrEmpty(userJson))
			{
				var userJsonC = JsonConvert.DeserializeObject<User>(userJson);
				userId = userJsonC.UserID;
			}
			
			var requests = _requestRepo.GetAllById(userId);
			
			// search
			if (!string.IsNullOrEmpty(searchQuery))
			{
				requests = _requestRepo.SearchIdFilter(searchQuery, filterQuery, userId);
			}
			//date filter
			if (!dateFrom.ToString("yyyy-MM-dd").Equals("0001-01-01"))
			{
				requests = await _requestRepo.GetRequestDateFilterAsync(requests, dateFrom, dateTo);
			} 
			//pagination
			var totalRequest = await requests.CountAsync();
			var totalPages = (int)Math.Ceiling((double)totalRequest / pageSize);
			var paginatedRequests = await _requestRepo.PaginateAsync(requests, pageNumber, pageSize);
			
			var dtos = _requestService.MapToListRequestOverviewDto(paginatedRequests);

			ViewBag.currentPage = pageNumber;
			ViewBag.totalPages = totalPages;
			return View(dtos);
		}
		public async Task<IActionResult> Detail(Guid id)
		{
			var request = await _requestRepo.GetAsync(r => r.RequestID.Equals(id));
			if (request == null) { return NotFound(); }
			return View(request);
		}
		public IActionResult Create()
		{
			var userJson = HttpContext.Session.GetString("user");
			var user  = JsonConvert.DeserializeObject<User>(userJson);

			ViewBag.UserEmail = user.UserEmail;
			ViewBag.UserPhoneNumber = user.UserPhoneNumber;
			ViewBag.UserAddress = user.UserAddress;
			return View();
		}
		
		[HttpPost]
		public async Task<IActionResult> Create(Request obj, List<IFormFile> images, 
			string? cityNameInput, string? districtNameInput, string? wardNameInput)
		{
			if (!Util.ValidateImage(images))
			{
				ModelState.AddModelError("Attachment", "Please provide valid images.");
			}
			if (wardNameInput.Equals("Choose ward/commune"))
			{
				ModelState.AddModelError("", "Please choose ward/commune of your request.");
			}
			if (districtNameInput.Equals("Choose district"))
			{
				ModelState.AddModelError("", "Please choose district of your request.");
			}
			if (cityNameInput.Equals("Choose province"))
			{
				ModelState.AddModelError("", "Please choose province of your request.");
			}
			
			var userJson = HttpContext.Session.GetString("user");
			var user = JsonConvert.DeserializeObject<User>(userJson);

			ViewBag.UserEmail = user.UserEmail;
			ViewBag.UserPhoneNumber = user.UserPhoneNumber;
			ViewBag.UserAddress = user.UserAddress;
    
			if (!ModelState.IsValid)
			{
				return View();
			}
			obj.RequestID = Guid.NewGuid();
			/*var date = DateOnly.FromDateTime(DateTime.Now);
			obj.CreationDate = date;*/
			obj.Location += ", " + wardNameInput + ", " + districtNameInput + ", " + cityNameInput;
			obj.UserID = user.UserID;
			if (images != null && images.Count > 0)
			{
				string imagePath = await _cloudinaryUploader.UploadMultiImagesAsync(images);
				obj.Attachment = imagePath;
			}
			else
			{
				obj.Attachment = "/images/Requests/Placeholder/xddFaker.png";
			}
			_logger.LogInformation("Request created");
			await _requestRepo.AddAsync(obj);
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
		[HttpPost]
		public async Task<IActionResult> Edit(Request obj, List<IFormFile> images, 
			string? cityNameInput, string? districtNameInput, string? wardNameInput)
		{
			if (!Util.ValidateImage(images))
			{
				ModelState.AddModelError("Attachment", "Please provide valid images.");
			}

			if (wardNameInput.Equals("Choose ward/commune"))
			{
				ModelState.AddModelError("", "Please choose ward/commune of your request.");
			}
			if (districtNameInput.Equals("Choose district"))
			{
				ModelState.AddModelError("", "Please choose district of your request.");
			}
			if (cityNameInput.Equals("Choose province"))
			{
				ModelState.AddModelError("", "Please choose province of your request.");
			}
			
			// Get the currently logged-in user (role and id)
			obj.Location += ", " + wardNameInput + ", " + districtNameInput + ", " + cityNameInput;
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Unauthorized();
			}
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
					existingRequest.Attachment = string.IsNullOrEmpty(existingRequest.Attachment) ? imagePath 
						: existingRequest.Attachment + "," + imagePath;
				}
			}
			if (!ModelState.IsValid)
			{
				return View(existingRequest);
			}
			_logger.LogInformation("Edit request");
			await _requestRepo.UpdateAsync(existingRequest);
			return RedirectToAction("MyRequest", "Request");
		}
		public async Task<IActionResult> Delete(Guid? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			Request request = await _requestRepo.GetAsync(r => r.RequestID.Equals(id));
			if (request == null) { return NotFound(); }
			return View(request);
		}
		[HttpPost, ActionName("Delete")]
		public async Task<IActionResult> DeletePost(Guid? id)
		{
			_logger.LogInformation("Get deleted request");
			Request request = await _requestRepo.GetAsync(r => r.RequestID.Equals(id));
			if (request == null) { return NotFound(); };
			_logger.LogInformation("Delete request");
			await _requestRepo.DeleteAsync(request);
			return RedirectToAction("MyRequest", "Request");
		}

		public async Task<IActionResult> AcceptRequest(Guid requestId)
		{
			return RedirectToAction("CreateProject", "Project", new { requestId = requestId });
		}
	}
}