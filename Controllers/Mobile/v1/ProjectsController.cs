using AonFreelancing.Contexts;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Entities;
using AonFreelancing.Models.Responses;
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
    public class ProjectsController(
        MainAppContext mainAppContext,
        UserManager<UserEntity> userManager,
        FileStorageService fileStorageService)
        : BaseController
    {
        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpPost]
        public async Task<IActionResult> PostProjectAsync([FromForm] ProjectInputDTO? projectInputDto)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var clientId = UtilitesMethods.GetUserId(HttpContext.User.Identity as ClaimsIdentity ?? throw new InvalidOperationException("User identity is null"));

            if (projectInputDto != null)
            {
                var project = new ProjectEntity
                {
                    ClientId = clientId,
                    Title = projectInputDto.Title,
                    Description = projectInputDto.Description,
                    QualificationName = projectInputDto.QualificationName,
                    Duration = projectInputDto.Duration,
                    Budget = projectInputDto.Budget,
                    PriceType = projectInputDto.PriceType,
                    CreatedAt = DateTime.Now,
                };

                var imageName = await fileStorageService.SaveFileAsync(projectInputDto.ImageFile);
                project.ImageName = imageName;
                
                await mainAppContext.Projects.AddAsync(project);
                await mainAppContext.SaveChangesAsync();
            }
            return StatusCode(StatusCodes.Status201Created, CreateSuccessResponse("Project added."));
        }

        [HttpGet("feed")]
        public async Task<IActionResult> GetClientFeedAsync(
            [FromQuery] List<string>? quls, [FromQuery] int page = 0,
            [FromQuery] int pageSize = 8, [FromQuery] string? qur = default
        )
        {
            var imageUrl = $"{Request.Scheme}://{Request.Host}/images";
            var trimmedQuery = qur?.ToLower().Replace(" ", "").Trim();
            var query = mainAppContext.Projects.AsQueryable();
            var count = await query.CountAsync();

            if (!string.IsNullOrEmpty(trimmedQuery))
            {
                query = query
                    .Where(p => p.Title.ToLower().Contains(trimmedQuery));
            }

            if (quls != null && quls.Count > 0)
            {
                query = query
                    .Where(p => quls.Contains(p.QualificationName));
            }
            // ORder by LAtest created
            var projects = await query.AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
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
                    Image = p.ImageName != null ? $"{imageUrl}/{p.ImageName}" : string.Empty,
                    CreatedAt = p.CreatedAt,
                    CreationTime = UtilitesMethods.GetTimeAgo(p.CreatedAt)
                })
                .ToListAsync();

            return Ok(CreateSuccessResponse(new
            {
                Total = count,
                Items = projects
            }));
        }

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpPost("{id}/bids")]
        public async Task<IActionResult> AddBidsAsync([FromRoute] long id, [FromBody] BidsInputDTO bidsInputDTO)
        {
            if (!ModelState.IsValid) return CustomBadRequest();

            var project = await mainAppContext.Projects.Include(p => p.Bids)
                .FirstOrDefaultAsync(p => p.Id == id);
            var freelancer = await userManager.GetUserAsync(HttpContext.User);

            if (freelancer == null || freelancer is not FreelancerEntity)
                return Forbid();

            if (project == null)
            {
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project not found."));
            }

            if (project.Status.Equals(Constants.PROJECT_STATUS_CLOSED))
            {
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "Project is closed."));
            }

            if (project.Budget <= bidsInputDTO.ProposedPrice)
            {
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "Proposed price is higher or equal than budget."));
            }

            if (project.Bids != null && project.Bids.Count > 0 && project.Bids
                .OrderBy(b => b.ProposedPrice)
                .FirstOrDefault()?.ProposedPrice <= bidsInputDTO.ProposedPrice)
            {
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "Proposed price is higher or equal than lowest bid."));
            }

            var bid = new BidsEntity
            {
                FreelancerId = freelancer.Id,
                ProjectId = project.Id,
                ProposedPrice = bidsInputDTO.ProposedPrice,
                Notes = bidsInputDTO.Notes ?? string.Empty,
                Status = Constants.BID_STATUS_PENDING,
                SubmittedAt = DateTime.Now
            };

            await mainAppContext.Bids.AddAsync(bid);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Bid added successfully"));
        }
    }
}