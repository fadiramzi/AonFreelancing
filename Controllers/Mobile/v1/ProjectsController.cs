
using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Responses;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/projects")]
    [ApiController]
    public class ProjectsController(MainAppContext mainAppContext, UserManager<User> userManager, ProjectLikeService projectLikeService,AuthService authService) : BaseController
    {
        [Authorize(Roles = "CLIENT")]
        [HttpPost]
        public async Task<IActionResult> PostProjectAsync([FromBody] ProjectInputDto projectInputDto)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            User? authenticatedUser = await userManager.GetUserAsync(HttpContext.User);
            if (authenticatedUser == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Unable to load user."));

            long clientId = authenticatedUser.Id;
            Project? newProject = Project.FromInputDTO(projectInputDto, clientId);

            await mainAppContext.Projects.AddAsync(newProject);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Project added."));
        }

        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpGet("clientfeed")]
        public async Task<IActionResult> GetClientFeedAsync(
            [FromQuery] List<string>? qualificationNames, [FromQuery] int page = 0,
            [FromQuery] int pageSize = 8, [FromQuery] string qur = ""
        )
        {
            string normalizedQuery = qur.ToLower().Replace(" ", "").Trim();
            List<ProjectOutDTO>? storedProjects;

            var query = mainAppContext.Projects.AsNoTracking().Include(p => p.Client).Include(p => p.ProjectLikes).AsQueryable();
            int totalProjectsCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(normalizedQuery))
                query = query.Where(p => p.Title.ToLower().Contains(normalizedQuery));

            if (qualificationNames != null && qualificationNames.Count > 0)
                query = query.Where(p => qualificationNames.Contains(p.QualificationName));

            storedProjects = await query.OrderByDescending(p => p.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(p => ProjectOutDTO.FromProject(p))
            .ToListAsync();

            return Ok(CreateSuccessResponse(new PaginatedResult<ProjectOutDTO>(totalProjectsCount, storedProjects)));
        }

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpGet("freelancerfeed")]
        public async Task<IActionResult> GetProjectFeedAsync(
            [FromQuery(Name = "specializations")] List<string>? qualificationNames,
            [FromQuery(Name = "timeline")] int? duration,
            [FromQuery] PriceRange priceRange,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 8,
            [FromQuery] string qur = ""
        )
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            string normalizedQuery = qur.ToLower().Replace(" ", "").Trim();
            var query = mainAppContext.Projects.AsNoTracking().Include(p => p.Client).Include(p => p.ProjectLikes).AsQueryable();
            int totalProjectsCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(normalizedQuery))
                query = query.Where(p => p.Title.ToLower().Contains(normalizedQuery));

            if (qualificationNames != null && qualificationNames.Count > 0)
                query = query.Where(p => qualificationNames.Contains(p.QualificationName));

            if (duration.HasValue)
                query = query.Where(p => p.Duration == duration.Value);

            if (priceRange.MinPrice != null && priceRange.MaxPrice != null)
                query = query.Where(p => p.Budget >= priceRange.MinPrice && p.Budget <= priceRange.MaxPrice);


            List<ProjectOutDTO>? storedProjects = await query.OrderByDescending(p => p.CreatedAt)
                                                             .Skip(page * pageSize)
                                                             .Take(pageSize)
                                                             .Select(p => ProjectOutDTO.FromProject(p))
                                                             .ToListAsync();
            return Ok(CreateSuccessResponse(new PaginatedResult<ProjectOutDTO>(totalProjectsCount, storedProjects)));
        }


        [Authorize(Roles = "FREELANCER")]
        [HttpPost("{projectId}/bids")]
        public async Task<IActionResult> SubmitBidAsync(long projectId, [FromBody] BidInputDto bidInputDTO)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            long authenticatedFreelancerId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            Project? storedProject = mainAppContext.Projects.Where(p => p.Id == projectId).Include(p => p.Bids).FirstOrDefault();

            if (storedProject == null) 
                return NotFound(CreateErrorResponse("404", "project not found"));
            if (storedProject.Status != Constants.PROJECT_STATUS_AVAILABLE)
                return Conflict(CreateErrorResponse("409", "cannot submit a bid for project that is not available for bids"));
            if (storedProject.Budget <= bidInputDTO.ProposedPrice) 
                return BadRequest(CreateErrorResponse("400", "proposed price must be less than the project's budget"));
            if (storedProject.Bids.Any() && storedProject.Bids.OrderBy(b => b.ProposedPrice).First().ProposedPrice <= bidInputDTO.ProposedPrice)
                return BadRequest(CreateErrorResponse("40", "proposed price must be less than earlier proposed prices"));

            Bid? newBid = Bid.FromInputDTO(bidInputDTO, authenticatedFreelancerId, projectId);
            await mainAppContext.AddAsync(newBid);
            await mainAppContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created);
        }


        [Authorize(Roles = "CLIENT")]
        [HttpPut("{projectId}/bids/{bidId}/approve")]
        public async Task<IActionResult> ApproveBidAsync([FromRoute] long projectId, [FromRoute] long bidId)
        {
           
            long authenticatedClientId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            Project? storedProject = await mainAppContext.Projects.Where(p => p.Id == projectId)
                                                                 .Include(p => p.Bids)
                                                                 .FirstOrDefaultAsync();

            if (storedProject == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "project not found"));

            if (authenticatedClientId != storedProject.ClientId)
                return Forbid();

            if (storedProject.Status != Constants.PROJECT_STATUS_AVAILABLE)
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "project status is not 'Available'"));
            
            Bid? storedBid = storedProject.Bids.Where(b => b.Id == bidId).FirstOrDefault();
            if (storedBid == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "bid not found"));

            storedBid.Status = Constants.BIDS_STATUS_APPROVED;
            storedBid.ApprovedAt = DateTime.Now;
            storedProject.Status = Constants.PROJECT_STATUS_CLOSED;
            storedProject.FreelancerId = storedBid.FreelancerId;

            await mainAppContext.SaveChangesAsync();
            return Ok();
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectDetailsAsync(long id)
        {
            var storedProject = await mainAppContext.Projects.Where(p => p.Id == id)
                                                        .Include(p => p.Bids)
                                                        .ThenInclude(b => b.Freelancer)
                                                        .FirstOrDefaultAsync();

            if (storedProject == null)
                return NotFound(CreateErrorResponse("404", "Project not found."));

            var orderedBids = storedProject.Bids
                .OrderByDescending(b => b.ProposedPrice)
                .Select(b => BidOutputDTO.FromBid(b));

            return Ok(CreateSuccessResponse(new
            {
                storedProject.Id,
                storedProject.Title,
                storedProject.Status,
                storedProject.Budget,
                storedProject.Duration,
                storedProject.Description,
                Bids = orderedBids
            }));
        }

        [Authorize(Roles = "CLIENT")]
        [HttpPost("{projectId}/tasks")]
        public async Task<IActionResult> CreateTaskAsync(long projectId, [FromBody] TaskInputDTO taskInputDTO)
        {
            Project? storedProject = await mainAppContext.Projects.FindAsync(projectId);
            if (storedProject == null || storedProject.Status != Constants.PROJECT_STATUS_CLOSED)
                return BadRequest(CreateErrorResponse("400", "Project not found or not closed."));

            TaskEntity? newTask = TaskEntity.FromInputDTO(taskInputDTO, projectId);

            await mainAppContext.Tasks.AddAsync(newTask);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Task created successfully."));
        }

        [HttpPost("{projectId}/likes")]
        public async Task<IActionResult> LikeOrUnLikeProject([FromRoute] long projectId, [AllowedValues(Constants.PROJECT_LIKE_ACTION, Constants.PROJECT_UNLIKE_ACTION)] string action)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            long authenticatedUserId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            ProjectLike? storedProjectLike = await mainAppContext.ProjectLikes.FirstOrDefaultAsync(pl => pl.ProjectId == projectId && pl.UserId == authenticatedUserId);

            if (storedProjectLike != null)
            {
                if (action == Constants.PROJECT_LIKE_ACTION)
                    return Conflict(CreateErrorResponse("409", "you cannot like the same project twice"));
                await projectLikeService.UnlikeProjectAsync(storedProjectLike);
                return NoContent();
            }
            if (storedProjectLike == null && action == Constants.PROJECT_LIKE_ACTION)
            {
                await projectLikeService.LikeProjectAsync(authenticatedUserId, projectId);
                return StatusCode(StatusCodes.Status201Created, "like submitted successfully");
            }
            return NoContent();
        }


        [Authorize(Roles = "CLIENT")]
        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadProjectImage(long id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error> { new Error { Code = "400", Message = "No file uploaded." } }
                });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error> { new Error { Code = "400", Message = "Invalid file type. Only image files are allowed." } }
                });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error> { new Error { Code = "400", Message = "File size exceeds the 5 MB limit." } }
                });
            }

            // Define the file path to save the image (e.g., in wwwroot/images)
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate a unique file name
            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadPath, fileName);

            // Save the image to the server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save the file metadata to the database 
            var project = await mainAppContext.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error> { new Error { Code = "404", Message = "Project not found." } }
                });
            }

            // Save the image path or filename to the project model
            project.ImagePath = $"/images/{fileName}";
            await mainAppContext.SaveChangesAsync();

            // Return a success response with the image URL or file path
            return Ok(new ApiResponse<string>
            {
                IsSuccess = true,
                Results = $"/images/{fileName}",
                Errors = null
            });
        }
        [HttpGet("{projectId}/tasks")]
        public async Task<IActionResult> GetTasksByProjectIdAsync([FromRoute] long projectId,
                                                                  [AllowedValues(Constants.TASK_STATUS_TO_DO,Constants.TASK_STATUS_DONE,Constants.TASK_STATUS_IN_PROGRESS,Constants.TASK_STATUS_IN_REVIEW,ErrorMessage = $"status should be one of the values: '{Constants.TASK_STATUS_TO_DO}', '{Constants.TASK_STATUS_DONE}', '{Constants.TASK_STATUS_IN_PROGRESS}', '{Constants.TASK_STATUS_IN_REVIEW}', or empty")]
                                                                    [FromQuery] string status = "")
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            List<TaskOutputDTO> storedTasksDTOs = await mainAppContext.Tasks.AsNoTracking()
                                                            .Where(t => t.ProjectId == projectId && t.Status.Contains(status))
                                                            .Select(t => TaskOutputDTO.FromTask(t))
                                                            .ToListAsync();
            return Ok(CreateSuccessResponse(storedTasksDTOs));
        }

    }
}