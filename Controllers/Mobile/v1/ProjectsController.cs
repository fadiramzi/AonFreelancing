using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
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
    public class ProjectsController(MainAppContext mainAppContext, UserManager<User> userManager) : BaseController
    {
        [Authorize(Roles = "CLIENT")]
        [HttpPost]
        public async Task<IActionResult> PostProjectAsync([FromBody] ProjectInputDto projectInputDto)
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
                CreatedAt = p.CreatedAt
                //CreationTime = StringOperations.GetTimeAgo(p.CreatedAt)
            })
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
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "project not found"));
            
            if (storedProject.Budget <= bidInputDTO.ProposedPrice)
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
            var storedProject = await mainAppContext.Projects.Where(p => p.Id == id)
                .Include(p => p.Bids)
                //.Include(p => p.Client)
                .Select(p => new ProjectOutDTO(p))
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
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),"Project not found"));
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
    }
}