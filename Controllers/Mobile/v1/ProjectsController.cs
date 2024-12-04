﻿
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
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/projects")]
    [ApiController]
    public class ProjectsController(MainAppContext mainAppContext, FileStorageService fileStorageService, UserManager<User> userManager, ProjectLikeService projectLikeService, AuthService authService) : BaseController
    {
        [Authorize(Roles = "CLIENT")]
        [HttpPost]
        public async Task<IActionResult> PostProjectAsync([FromForm] ProjectInputDto projectInputDto)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            User? authenticatedUser = await userManager.GetUserAsync(HttpContext.User);
            if (authenticatedUser == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Unable to load user."));

            long clientId = authenticatedUser.Id;
            Project? newProject = Project.FromInputDTO(projectInputDto, clientId);

            if (projectInputDto.ImageFile != null)
                newProject.ImageFileName = await fileStorageService.SaveAsync(projectInputDto.ImageFile);

            await mainAppContext.Projects.AddAsync(newProject);
            await mainAppContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProjectDetailsAsync),new {id = newProject.Id},null);
        }

        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpGet("clientfeed")]
        public async Task<IActionResult> GetClientFeedAsync(
            [FromQuery] List<string>? qualificationNames, [FromQuery] int page = 0,
            [FromQuery] int pageSize = 8, [FromQuery] string qur = ""
        )
        {
            string imagesBaseUrl = $"{Request.Scheme}://{Request.Host}/images";
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
            .Select(p => ProjectOutDTO.FromProject(p, imagesBaseUrl))
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

            string imagesBaseUrl = $"{Request.Scheme}://{Request.Host}/images";
            string normalizedQuery = qur.ToLower().Replace(" ", "").Trim();
            var query = mainAppContext.Projects.AsNoTracking().Include(p => p.Client).Include(p => p.ProjectLikes).AsQueryable();
            int totalProjectsCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(normalizedQuery))
                query = query.Where(p => p.Title.ToLower().Contains(normalizedQuery));

            if (qualificationNames != null && qualificationNames.Count > 0)
                query = query.Where(p => qualificationNames.Contains(p.QualificationName));

            if (duration.HasValue)
                query = query.Where(p => p.Duration >= duration.Value);

            if (priceRange.MinPrice != null && priceRange.MaxPrice != null)
                query = query.Where(p => p.Budget >= priceRange.MinPrice && p.Budget <= priceRange.MaxPrice);


            List<ProjectOutDTO>? storedProjects = await query.OrderByDescending(p => p.CreatedAt)
                                                             .Skip(page * pageSize)
                                                             .Take(pageSize)
                                                             .Select(p => ProjectOutDTO.FromProject(p, imagesBaseUrl))
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
                                                        .Include(p => p.Tasks)
                                                        .Include(p => p.Bids)
                                                        .ThenInclude(b => b.Freelancer)
                                                        .FirstOrDefaultAsync();

            if (storedProject == null)
                return NotFound(CreateErrorResponse("404", "Project not found."));

            int numberOfCompletedTasks = storedProject.Tasks.Where(t => t.Status == Constants.TASK_STATUS_DONE).ToList().Count;
            decimal totalNumberOFTasks = storedProject.Tasks.Count;
            decimal percentage = 0;
            if (totalNumberOFTasks > 0)
                percentage = (numberOfCompletedTasks / totalNumberOFTasks) * 100;

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
                Percentage = percentage,
                Bids = orderedBids
            }));
        }

        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpPost("{projectId}/tasks")]
        public async Task<IActionResult> CreateTaskAsync(long projectId, [FromBody] TaskInputDTO taskInputDTO)
        {
            long authenticatedClientId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            Project? storedProject = await mainAppContext.Projects.AsNoTracking().FirstOrDefaultAsync(p=>p.Id == projectId);
            if (storedProject == null )
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project not found"));
            if (authenticatedClientId != storedProject.ClientId)
                return Forbid();
            if(storedProject.Status != Constants.PROJECT_STATUS_CLOSED)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "project is not status closed yet"));
            

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

        [HttpGet("{projectId}/tasks")]
        public async Task<IActionResult> GetTasksByProjectIdAsync([FromRoute] long projectId,
                                                                  [AllowedValues(Constants.TASK_STATUS_TO_DO,Constants.TASK_STATUS_DONE,Constants.TASK_STATUS_IN_PROGRESS,Constants.TASK_STATUS_IN_REVIEW,ErrorMessage = $"status should be one of the values: '{Constants.TASK_STATUS_TO_DO}', '{Constants.TASK_STATUS_DONE}', '{Constants.TASK_STATUS_IN_PROGRESS}', '{Constants.TASK_STATUS_IN_REVIEW}', or empty")]
                                                                    [FromQuery] string status = "")
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();
            
            long authenticatedUserId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            Project? storedProject = await mainAppContext.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);

            if (storedProject == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "project not found"));
            if (authenticatedUserId != storedProject.ClientId && authenticatedUserId != storedProject.FreelancerId)
                return Forbid();

            List<TaskOutputDTO> storedTasksDTOs = await mainAppContext.Tasks.AsNoTracking()
                                                            .Where(t => t.ProjectId == projectId && t.Status.Contains(status))
                                                            .Select(t => TaskOutputDTO.FromTask(t))
                                                            .ToListAsync();
            return Ok(CreateSuccessResponse(storedTasksDTOs));
        }

    }
}