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
    public class ProjectsController(MainAppContext mainAppContext, UserManager<User> userManager, FileVaildation fileVaildation) : BaseController
    {
        [Authorize(Roles = "CLIENT")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PostProjectAsync([FromForm] ProjectInputDto projectInputDto)
        {
            if(!ModelState.IsValid)
            {
                
                return base.CustomBadRequest();
            }
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Unable to load user."));

            var project = new Project
            {
                ClientId = user.Id,
                Title = projectInputDto.Title,
                Description = projectInputDto.Description,
                QualificationName = projectInputDto.QualificationName,
                Duration = projectInputDto.Duration,
                Budget = projectInputDto.Budget,
                PriceType = projectInputDto.PriceType,
                CreatedAt = DateTime.Now,
            };

            if (projectInputDto.Image != null)
            {
                string fileName = await fileVaildation.ValidateImageFileAsync(projectInputDto.Image);
                project.Image = fileName;
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
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

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

            // ORder by LAtest created
            projects = await query.OrderByDescending(p => p.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectOutDTO
            {

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
                CreationTime = StringOperations.GetTimeAgo(p.CreatedAt),
                Image = $"{baseUrl}/Uploads/{p.Image}"
            })
            .ToListAsync();
           
            return Ok(CreateSuccessResponse(new { 
                Total=count,
                Items=projects
            }));
        }


        [HttpPost("{id}/bids")]
        [Authorize(Roles = "FREELANCER")]
        public async Task<IActionResult> BidsSubmit(int id, [FromBody] BidInputDTO bidInputDTO)
        {
            if (!ModelState.IsValid)
            {
                return base.CustomBadRequest();
            }

            var freelancer = await userManager.GetUserAsync(HttpContext.User);

            var project = await mainAppContext.Projects
                .Where(p => p.Id == id)
                .Include(p => p.Bids)
                .FirstOrDefaultAsync();

            if (project == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project is not found !"));

            if (project.Budget <= bidInputDTO.ProposedPrice)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "Proposed price must be less than the project budget !"));

            // if there is bids the new bid should be less than the last bid
            var lastBid = project.Bids.OrderByDescending(b => b.SubmittedAt).FirstOrDefault();

            if (lastBid != null && bidInputDTO.ProposedPrice >= lastBid.ProposedPrice)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), 
                                    $"Proposed price must be less than the last bid : {lastBid.ProposedPrice} !"));

            var bid = new Bid
            {
                ProjectId = id,
                FreelancerId = freelancer.Id,
                ProposedPrice = bidInputDTO.ProposedPrice,
                Notes = bidInputDTO.Notes,
                SubmittedAt = DateTime.Now
            };

            await mainAppContext.Bids.AddAsync(bid);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("New Bid is Submitted"));
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var project = await mainAppContext.Projects
                 .Include(p => p.Bids.OrderBy(b => b.ProposedPrice))  
                 .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project is not found !"));

            var projectResponse = new ProjectBidsDTO
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                Budget = project.Budget,
                Status = project.Status,
                Duration = project.Duration,
                CreatedAt = project.CreatedAt,
                Bids = project.Bids.Select(b => new BidOutputDTO
                {
                    ProposedPrice = b.ProposedPrice,
                    Notes = b.Notes,
                    Status = b.Status,
                    SubmittedAt = b.SubmittedAt
                }).ToList()
            };

            return Ok(CreateSuccessResponse(projectResponse));

        }

        [HttpPatch("{pid}/bids/{bid}/approve")]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> ApproveBid(int pid, int bid)
        {
            var project = await mainAppContext.Projects
                .Include(p => p.Bids)
                .FirstOrDefaultAsync(p => p.Id == pid);

            if (project == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project is not found !"));

            if (project.Status == "Closed")
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "Project is closed !"));

            var bidToApprove = await mainAppContext.Bids
                .FirstOrDefaultAsync(b => b.Id == bid && b.ProjectId == pid && b.Status != "pending");

            if (bidToApprove == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Bid not found !"));

            bidToApprove.Status = "approved";
            bidToApprove.ApprovedAt = DateTime.Now;

            project.Status = "Closed";

            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Bid approved successfully"));
        }

        [HttpPost("{id}/tasks")]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> CreateTask(int id, [FromBody] TaskInputDTO taskInputDTO)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            var user = await userManager.GetUserAsync(HttpContext.User);
            var project = await mainAppContext.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.ClientId == user.Id);

            if (project == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project is not found !"));

            var task = new Models.Tasks
            {
                ProjectId = id,
                Name = taskInputDTO.Name,
                DeadlineAt = taskInputDTO.DeadlineAt,
                Notes = taskInputDTO.Notes
            };

            await mainAppContext.Tasks.AddAsync(task);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Task Created successfully"));
        }
    }
}