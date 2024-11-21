using AonFreelancing.Contexts;
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
    public class ProjectsController(MainAppContext mainAppContext, UserManager<User> userManager,FileStorageService fileStorageService) : BaseController
    {
        [Authorize(Roles = "CLIENT")]
        [HttpPost]
        public async Task<IActionResult> PostProjectAsync([FromForm] ProjectInputDto projectInputDTO)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Unable to load user."));

            long userId = user.Id;
            Project newProject = new Project
            {
                ClientId = userId,
                Title = projectInputDTO.Title,
                Description = projectInputDTO.Description,
                QualificationName = projectInputDTO.QualificationName,
                Duration = projectInputDTO.Duration,
                Budget = projectInputDTO.Budget,
                PriceType = projectInputDTO.PriceType,
                CreatedAt = DateTime.Now,
            };
            if (projectInputDTO.ImageFile != null)
            {
                string fileName = await fileStorageService.SaveAsync(projectInputDTO.ImageFile);
                newProject.ImageFileName = fileName;
            }
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
            if (!ModelState.IsValid)
                return base.CustomBadRequest();
            var trimmedQuery = qur?.ToLower().Replace(" ", "").Trim();
            List<ProjectOutDTO>? projects;
            var imagesBaseUrl = $"{Request.Scheme}://{Request.Host}/images";

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

            // ORder by LAtest created
            projects = await query.OrderByDescending(p => p.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectOutDTO(p,imagesBaseUrl))
            .ToListAsync();
           
            return Ok(CreateSuccessResponse(new { 
                Total=count,
                Items=projects
            }));
        }

        [Authorize(Roles ="FREELANCER")]
        [HttpPost("{projectId}/bids")]
        public async Task<IActionResult> CreateBidAsync([FromRoute]long projectId, [FromBody] BidInputDTO bidInputDTO)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long freelancerId = Convert.ToInt64(identity.FindFirst(ClaimTypes.NameIdentifier).Value);

            Project? storedProject = mainAppContext.Projects.Where(p => p.Id == projectId).Include(p=>p.Bids).FirstOrDefault();
            string? errorMessage = storedProject switch
            {
                _ when storedProject is null => "invalid project id",
                _ when storedProject.Bids.Any(b=>b.FreelancerId == freelancerId) => "you have already submitted a bid for this project",
                _ when storedProject.Status != Constants.PROJECT_STATUS_AVAILABLE => "cannot POST a bid for a project that is not available for bids",
                _ when storedProject.Budget <= bidInputDTO.ProposedPrice => "proposed price must be less than the project's budget",
                _ when storedProject.Bids.Any() && storedProject.Bids.OrderBy(b => b.ProposedPrice).First().ProposedPrice <= bidInputDTO.ProposedPrice => "proposed price must be less than earlier proposed prices",
                _ => null
            };
            if (errorMessage != null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), errorMessage));

            var newBid = new Bid(bidInputDTO, projectId, freelancerId);
            await mainAppContext.AddAsync(newBid);
            await mainAppContext.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles ="CLIENT")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectAsync(int id)
        {
            string imagesBaseUrl = $"{Request.Scheme}://{Request.Host}/images";

            var storedProjectOutDTO = await mainAppContext.Projects.AsNoTracking()
                                                                    .Where(p => p.Id == id)
                                                                    .Include(p => p.Bids)
                                                                    .Select(p => new ProjectOutDTO(p,imagesBaseUrl))
                                                                    .FirstOrDefaultAsync();

            return Ok(CreateSuccessResponse(storedProjectOutDTO));
        }

        [Authorize(Roles ="CLIENT")]
        [HttpPatch("{projectId}/bids/{bidId}/approve")]
        public async Task<IActionResult> ApproveBidAsync(long projectId, long bidId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long clientId = Convert.ToInt64(identity.FindFirst(ClaimTypes.NameIdentifier).Value);

           Project? storedProject = await mainAppContext.Projects.Where(p => p.Id == projectId)
                                                                .Include(p => p.Bids)
                                                                .FirstOrDefaultAsync();
            Bid? storedBid = null;

            string? badRequestErrorMessage = storedProject switch
            {
                _ when storedProject is null => "invalid  project id",
                _ when storedProject.Status != Constants.PROJECT_STATUS_AVAILABLE => "project status is not 'Available'",
                _ when (storedBid = storedProject.Bids.Where(b=>b.Id == bidId).FirstOrDefault()) is null => "invalid bid id",
                _ => null
            };
            if (badRequestErrorMessage != null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), badRequestErrorMessage));

            storedBid.Status = Constants.BID_STATUS_APPROVED;
            storedBid.ApprovedAt = DateTime.Now;
            storedProject.Status = Constants.PROJECT_STATUS_CLOSED;

            await mainAppContext.SaveChangesAsync();
            return NoContent();

        }
        [Authorize(Roles ="CLIENT")]
        [HttpPost("{projectId}/tasks")]
        public async Task<IActionResult> CreateTaskAsync([FromRoute] long projectId, [FromBody] TaskInputDTO taskInputDTO)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long clientId = Convert.ToInt64(identity.FindFirst(ClaimTypes.NameIdentifier).Value);

            Models.Task newTask = new Models.Task(taskInputDTO, projectId, clientId);
            await mainAppContext.AddAsync(newTask);
            await mainAppContext.SaveChangesAsync();

            return CreatedAtAction(nameof(TasksController.GetByIdAsync),"Tasks", new {id = newTask.Id }, new TaskOutputDTO(newTask));
        }
        [HttpGet("{projectId}/tasks")]
        public async Task<IActionResult> GetTasksByProjectIdAsync([FromRoute] long projectId,
                                                                [AllowedValues("to-do", "done", "in-progress", "in-review",ErrorMessage = "status should be one of the values: 'to-do', 'done', 'in-progress', 'in-review'")][FromQuery] string status = "")
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            var storedTasksDTOs = await mainAppContext.Tasks.AsNoTracking()
                                                            .Where(t => t.ProjectId == projectId && t.Status.Contains(status))
                                                            .Select(t => new TaskOutputDTO(t))
                                                            .ToListAsync();
                return Ok(CreateSuccessResponse(storedTasksDTOs));
        }


    }
}