using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            var userId = user.Id;
            var project = new Project
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
                project.ImageFileName = fileName;
            }
            await mainAppContext.Projects.AddAsync(project);
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
            .Select(p => new ProjectOutDTO(p,imagesBaseUrl)
            //{
            //    Title = p.Title,
            //    Description = p.Description,
            //    Status = p.Status,
            //    Budget = p.Budget,
            //    Duration = p.Duration,
            //    PriceType = p.PriceType,
            //    Qualifications = p.QualificationName,
            //    StartDate = p.StartDate,
            //    EndDate = p.EndDate,
            //    CreatedAt = p.CreatedAt
            //    //CreationTime = StringOperations.GetTimeAgo(p.CreatedAt)
            //}
            )
            .ToListAsync();
           
            return Ok(CreateSuccessResponse(new { 
                Total=count,
                Items=projects
            }));
        }

        [Authorize(Roles ="FREELANCER")]
        [HttpPost("{projectId}/bids")]
        public async Task<IActionResult> CreateBid([FromRoute]long projectId, [FromBody] BidInputDTO bidInputDTO)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            Project? storedProject = mainAppContext.Projects.Where(p => p.Id == projectId).Include(p=>p.Bids).FirstOrDefault();
            if (storedProject == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "invalid project id"));

            if (storedProject.Budget <= bidInputDTO.ProposedPrice)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "proposed price must be less than the project's budget"));
            
            if(storedProject.Status == Constants.PROJECT_STATUS_AVAILABLE)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "proposed price must be less than the project's budget"));
            
            Bid? storedBidWithLowestPrice = storedProject.Bids.OrderBy(b => b.ProposedPrice).FirstOrDefault();
            if (storedBidWithLowestPrice != null && storedBidWithLowestPrice.ProposedPrice <= bidInputDTO.ProposedPrice)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "proposed price should be less than earlier proposed prices"));

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long freelancerId = Convert.ToInt64(identity.FindFirst(ClaimTypes.NameIdentifier).Value);
            Bid newBid = new Bid(bidInputDTO, projectId, freelancerId);

            await mainAppContext.AddAsync(newBid);
            await mainAppContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created);
        }
        [Authorize(Roles ="CLIENT")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var imagesBaseUrl = $"{Request.Scheme}://{Request.Host}/images";

            var storedProject = await mainAppContext.Projects.Where(p => p.Id == id)
                .Include(p => p.Bids)
                //.Include(p => p.Client)
                .Select(p => new ProjectOutDTO(p,imagesBaseUrl))
                .FirstOrDefaultAsync();

            return Ok(CreateSuccessResponse(storedProject));

        }

        [Authorize(Roles ="CLIENT")]
        [HttpPatch("{projectId}/bids/{bidId}/approve")]
        public async Task<IActionResult> ApproveBid(long projectId, long bidId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long clientId = Convert.ToInt64(identity.FindFirst(ClaimTypes.NameIdentifier).Value);

           Project? storedProject = await mainAppContext.Projects.Where(p => p.Id == projectId)
                                                                .Include(p => p.Bids)
                                                                .FirstOrDefaultAsync();
            Error? error = null;
            if(storedProject == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "invalid  project id"));
            if (storedProject.Status != Constants.PROJECT_STATUS_AVAILABLE)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "project status is not 'Available'"));

            Bid? storedBid = storedProject.Bids.Where(b => b.Id == bidId).FirstOrDefault();
            if (storedBid == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Bid not found"));

            storedBid.Status = Constants.BID_STATUS_APPROVED;
            storedBid.ApprovedAt = DateTime.Now;
            storedProject.Status = Constants.PROJECT_STATUS_CLOSED;

            await mainAppContext.SaveChangesAsync();
            return NoContent();

        }
        [Authorize(Roles ="CLIENT")]
        [HttpPost("{projectId}/tasks")]
        public async Task<IActionResult> CreateTask([FromRoute] long projectId, [FromBody] TaskInputDTO taskInputDTO)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            Models.Task newTask = new Models.Task(taskInputDTO, projectId);
            await mainAppContext.AddAsync(newTask);
            await mainAppContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaskById), new {projectId = projectId, taskId = newTask.Id }, new TaskOutputDTO(newTask));
        }
        [HttpGet("{projectId}/tasks")]
        public async Task<IActionResult> GetTasksByProjectId(long projectId)
        {
            var storedTasksDTOs = await mainAppContext.Tasks.Where(t => t.ProjectId== projectId).Select(t => new TaskOutputDTO(t)).ToListAsync();
                return Ok(CreateSuccessResponse(storedTasksDTOs));
        }

        [HttpGet("{projectId}/tasks/{taskId}")]
        public async Task<IActionResult> GetTaskById(long taskId)
        {
            var storedTask = await mainAppContext.Tasks.Where(t => t.Id == taskId).FirstOrDefaultAsync();
            if (storedTask != null)
                return Ok(CreateSuccessResponse(new TaskOutputDTO(storedTask)));

            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),"Task not found"));
        }  

        [HttpPatch("{projectId}/tasks/{taskId}")]
        public async Task<IActionResult> UpdateTaskById(long taskId, [FromBody] TaskUpdateDTO taskUpdateDTO)
        {
            var storedTask = await mainAppContext.Tasks.Where(t => t.Id == taskId).FirstOrDefaultAsync();
            if (storedTask == null)
            return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),"invalid task id"));

            if(taskUpdateDTO.Status == Constants.TASK_STATUS_DONE)
                storedTask.CompletedAt = DateTime.Now;   

            storedTask.Status = taskUpdateDTO.Status;
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse(new TaskOutputDTO(storedTask)));
        }
    }
}