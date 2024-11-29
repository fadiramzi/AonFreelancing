﻿using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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
            {
                
                return base.CustomBadRequest();
            }
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Unable to load user."));

            var userId = user.Id;
            var project = new Project
            {
                ClientId = userId,
                Title = projectInputDto.Title,
                Description = projectInputDto.Description,
                QualificationName = projectInputDto.QualificationName,
                Duration = projectInputDto.Duration,
                Budget = projectInputDto.Budget,
                PriceType = projectInputDto.PriceType,
                CreatedAt = DateTime.Now,
            };

            await mainAppContext.Projects.AddAsync(project);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Project added."));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectDetailsAsync(long id)
        {
            var project = await mainAppContext.Projects
                .Where(p => p.Id == id)
                .Include(p => p.Bids)
                .ThenInclude(b => b.Freelancer)
                .FirstOrDefaultAsync();

            if (project == null)
                return NotFound(CreateErrorResponse("404", "Project not found."));

            var orderedBids = project.Bids
                .OrderByDescending(b => b.ProposedPrice)
                .Select(b => new BidOutDto
                {
                    Id = b.Id,
                    FreelancerId = b.FreelancerId,
                    Freelancer = new FreelancerShortOutDTO
                    {
                        Id = b.FreelancerId,
                        Name = b.Freelancer.Name
                    },
                    ProposedPrice = b.ProposedPrice,
                    Notes = b.Notes,
                    Status = b.Status,
                    SubmittedAt = b.SubmittedAt,
                    ApprovedAt = b.ApprovedAt
                });



            return Ok(CreateSuccessResponse(new
            {
                project.Id,
                project.Title,
                project.Status,
                project.Budget,
                project.Duration,
                project.Description,
                Bids = orderedBids
            }));
        }


        [Authorize(Roles = "CLIENT")]
        [HttpGet("clientFeed")]
        public async Task<IActionResult> GetClientFeedAsync(
            [FromQuery] List<string>? qualificationNames, [FromQuery] int page = 0,
            [FromQuery] int pageSize = 8, [FromQuery] string? qur = default
        )
        {
            var trimmedQuery = qur?.ToLower().Replace(" ", "").Trim();
            List<ProjectOutDTO>? projects;

            var query = mainAppContext.Projects.AsQueryable();

            var count = await query.CountAsync();

            if(!string.IsNullOrEmpty(trimmedQuery))
            {
                query = query
                    .Where(p=>p.Title.ToLower().Contains(trimmedQuery));
            }
            if(qualificationNames != null && qualificationNames.Count >0)
            {
                query = query
                    .Where(p => qualificationNames.Contains(p.QualificationName));
            }

            projects = await query.OrderByDescending(p => p.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectOutDTO
            {
                Id= p.Id,
                Title = p.Title,
                Description = p.Description,
                Status = p.Status,
                Budget = p.Budget,
                Duration = p.Duration,
                PriceType = p.PriceType,
                Qualifications = p.QualificationName,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CreatedAt = p.CreatedAt,
                CreationTime = StringOperations.GetTimeAgo(p.CreatedAt)
            })
            .ToListAsync();
           
            return Ok(CreateSuccessResponse(new { 
                Total=count,
                Items=projects
            }));
        }

        [Authorize(Roles = "FREELANCER")]
        [HttpPost("{id}/bids")]
        public async Task<IActionResult> SubmitBidAsync(long id, [FromBody] BidInputDto bidDto)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var project = await mainAppContext.Projects.FindAsync(id);
            if (project == null)
                return NotFound(CreateErrorResponse("404", "Project not found."));

            if (project.Status == Constants.PROJECT_STATUS_CLOSED)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                   "project is closed "));

            var user = await userManager.GetUserAsync(User);
            //if (user == null || !User.IsInRole("FREELANCER"))
            //    return Forbid();

            var lastBid = await mainAppContext.Bids
                .Where(b => b.ProjectId == id)
                .OrderByDescending(b => b.SubmittedAt)
                .FirstOrDefaultAsync();

            if (bidDto.ProposedPrice <= 0 ||
                (lastBid != null && bidDto.ProposedPrice > lastBid.ProposedPrice) ||
                (lastBid == null && bidDto.ProposedPrice > project.Budget))
            {
                return BadRequest(CreateErrorResponse("400", "Invalid proposed price. The proposed price must be positive and lower than the last bid or project budget."));
            }

            var bid = new Bid
            {
                ProjectId = id,
                FreelancerId = user.Id,
                ProposedPrice = bidDto.ProposedPrice,
                Notes = bidDto.Notes,
                Status = Constants.BIDS_STATUS_PENDING, 
                SubmittedAt = DateTime.Now
            };

            await mainAppContext.Bids.AddAsync(bid);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Bid submitted successfully."));
        }


        [Authorize(Roles = "CLIENT")]
        [HttpPut("{pid}/bids/{bid}/approve")]
        public async Task<IActionResult> ApproveBidAsync(long pid, long bid)
        {
            var project = await mainAppContext.Projects.FindAsync(pid);
            if (project == null || project.Status != Constants.PROJECT_STATUS_AVAILABLE)
                return BadRequest(CreateErrorResponse("400", $"Project status is '{project?.Status}', but must be 'available'."));

            var bidID = await mainAppContext.Bids.FirstOrDefaultAsync(b => b.Id == bid);
            if (bidID == null || bidID.ProjectId != pid || bidID.Status == Constants.BIDS_STATUS_APPROVED)
                return BadRequest(CreateErrorResponse("400", "Bid not found or already approved."));

            bidID.Status = Constants.BIDS_STATUS_APPROVED;
            bidID.ApprovedAt = DateTime.Now;

            project.Status = Constants.PROJECT_STATUS_CLOSED;
            project.FreelancerId = bidID.FreelancerId;
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Bid approved successfully."));
        }


        [Authorize(Roles = "CLIENT")]
        [HttpPost("{id}/tasks")]
        public async Task<IActionResult> CreateTaskAsync(long id, [FromBody] TaskInputDto taskDto)
        {
            var project = await mainAppContext.Projects.FindAsync(id);
            if (project == null || project.Status != Constants.PROJECT_STATUS_CLOSED)
                return BadRequest(CreateErrorResponse("400", "Project not found or not closed."));

            var task = new TaskEntity
            {
                ProjectId = id,
                Name = taskDto.Name,
                DeadlineAt = taskDto.DeadlineAt,
                Notes = taskDto.Notes
            };

            await mainAppContext.Tasks.AddAsync(task);
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

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

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

            project.ImagePath = $"/images/{fileName}";
            await mainAppContext.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                IsSuccess = true,
                Results = $"/images/{fileName}",
                Errors = null
            });
        }


    [Authorize(Roles = "FREELANCER")]
    [HttpGet("filter")]
        public async Task<IActionResult> GetProjectsFilterAsync(
            [FromQuery] string? qualificationName,
            [FromQuery] decimal? minBudget,
            [FromQuery] decimal? maxBudget,
            [FromQuery] int? timeLine)
        {
            var query = mainAppContext.Projects.AsQueryable();

            if (!string.IsNullOrEmpty(qualificationName))
            {
                query = query.Where(p => p.QualificationName != null &&
                                            p.QualificationName.ToLower().Contains(qualificationName.ToLower()));
            }

            if (minBudget.HasValue)
            {
                query = query.Where(p => p.Budget >= minBudget.Value);
            }

            if (maxBudget.HasValue)
            {
                query = query.Where(p => p.Budget <= maxBudget.Value);
            }

            if (timeLine.HasValue)
            {
                query = query.Where(p => p.Duration <= timeLine.Value);
            }

            var filteredProjects = await query.ToListAsync();

            return Ok(CreateSuccessResponse(filteredProjects));
        
        }

        // /api/mobile/v1/projects/{pid}/like
        [Authorize(Roles = "CLIENT,FREELANCER")]
        [HttpPost("{pid}/like")]
        public async Task<IActionResult> LikeProjectAsync(long pid, string status)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            if (!ModelState.IsValid)
            {
                return base.CustomBadRequest();
            }

            var projectLike = await mainAppContext.ProjectLikes
                .FirstOrDefaultAsync(l => l.ProjectId == pid && l.UserId == user.Id);

            if (status == Constants.PROJECT_LIKE)
            {
                if (projectLike == null)
                {
                    var like = new ProjectLike
                    {
                        ProjectId = pid,
                        UserId = user.Id,
                        CreatedAt = DateTime.Now
                    };

                    await mainAppContext.ProjectLikes.AddAsync(like);
                    await mainAppContext.SaveChangesAsync();
                    return Ok(CreateSuccessResponse(like));
                }

                return BadRequest(CreateErrorResponse(
                    StatusCodes.Status400BadRequest.ToString(),
                    "You already liked this project"));
            }

            if (status == Constants.PROJECT_UNLIKE)
            {
                if (projectLike != null)
                {
                    mainAppContext.ProjectLikes.Remove(projectLike);
                    await mainAppContext.SaveChangesAsync();
                    return Ok(CreateSuccessResponse("unliked"));
                }

                return BadRequest(CreateErrorResponse(
                    StatusCodes.Status400BadRequest.ToString(),
                    "You haven't liked this project"));
            }

            return BadRequest(CreateErrorResponse(
                StatusCodes.Status400BadRequest.ToString(),
                "Invalid status, Use 'like' or 'unlike'."));
        }

        //[HttpGet("{id}")]
        //public IActionResult GetProject(int id)
        //{
        //    var project = _mainAppContext.Projects
        //        .Include(p => p.Client)
        //        .FirstOrDefault(p => p.Id == id);

        //    return Ok(CreateSuccessResponse(project));

        //}
    }
}