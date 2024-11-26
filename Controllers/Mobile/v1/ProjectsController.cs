
using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/projects")]
    [ApiController]
    public class ProjectsController(MainAppContext mainAppContext, UserManager<User> userManager) : BaseController
    {
        [Authorize(Roles = "CLIENT")]
        [HttpPost]
        public async Task<IActionResult> PostProjectAsync([FromBody] ProjectInputDto projectInputDto)
        {
            if(!ModelState.IsValid)
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

        [Authorize(Roles = "CLIENT")]
        [HttpGet("clientFeed")]
        public async Task<IActionResult> GetClientFeedAsync(
            [FromQuery] List<string>? qualificationNames, [FromQuery] int page = 0,
            [FromQuery] int pageSize = 8, [FromQuery] string? qur = default
        )
        {
            var trimmedQuery = qur?.ToLower().Replace(" ", "").Trim();
            List<ProjectOutDTO>? storedProjects;

            var query = mainAppContext.Projects.AsQueryable();

            var count = await query.CountAsync();

            if(!string.IsNullOrEmpty(trimmedQuery))
                query = query.Where(p=>p.Title.ToLower().Contains(trimmedQuery));

            if (qualificationNames != null && qualificationNames.Count > 0)
                query = query.Where(p => qualificationNames.Contains(p.QualificationName));

            storedProjects = await query.OrderByDescending(p => p.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(p => ProjectOutDTO.FromProject(p))
            .ToListAsync();
           
            return Ok(CreateSuccessResponse(new { 
                Total=count,
                Items=storedProjects
            }));
        }


        [Authorize(Roles = "FREELANCER")]
        [HttpPost("{projectId}/bids")]
        public async Task<IActionResult> SubmitBidAsync(long projectId, [FromBody] BidInputDto bidInputDTO)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            Project? storedProject = await mainAppContext.Projects.FindAsync(projectId);
            if (storedProject == null)
                return NotFound(CreateErrorResponse("404", "Project not found."));

            User authenticatedUser = await userManager.GetUserAsync(User);

            Bid? latestBid = await mainAppContext.Bids.Where(b => b.ProjectId == projectId)
                                                    .OrderByDescending(b => b.SubmittedAt)
                                                    .FirstOrDefaultAsync();

            if (bidInputDTO.ProposedPrice <= 0 ||
                (latestBid != null && bidInputDTO.ProposedPrice > latestBid.ProposedPrice) ||
                (latestBid == null && bidInputDTO.ProposedPrice > storedProject.Budget))
            {
                return BadRequest(CreateErrorResponse("400", "Invalid proposed price. The proposed price must be positive and lower than the last bid or project budget."));
            }

            Bid? newBid = Bid.FromInputDTO(bidInputDTO, authenticatedUser.Id, storedProject.Id);

            await mainAppContext.Bids.AddAsync(newBid);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Bid submitted successfully."));
        }


        [Authorize(Roles = "CLIENT")]
        [HttpPut("{projectId}/bids/{bidId}/approve")]
        public async Task<IActionResult> ApproveBidAsync(long projectId, long bidId)
        {
            Project? storedProject = await mainAppContext.Projects.FindAsync(projectId);
            if (storedProject == null || storedProject.Status != Constants.PROJECT_STATUS_AVAILABLE)
                return BadRequest(CreateErrorResponse("400", $"Project status is '{storedProject?.Status}', but must be 'available'."));

            var storedBid = await mainAppContext.Bids.FirstOrDefaultAsync(b => b.Id == bidId);
            if (storedBid == null || storedBid.ProjectId != projectId || storedBid.Status == Constants.BIDS_STATUS_APPROVED)
                return BadRequest(CreateErrorResponse("400", "Bid not found or already approved."));

            storedBid.Status = Constants.BIDS_STATUS_APPROVED;
            storedBid.ApprovedAt = DateTime.Now;
            storedProject.Status = Constants.PROJECT_STATUS_CLOSED;
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Bid approved successfully."));
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

            TaskEntity? newTask = TaskEntity.FromInputDTO(taskInputDTO,projectId);

            await mainAppContext.Tasks.AddAsync(newTask);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Task created successfully."));
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