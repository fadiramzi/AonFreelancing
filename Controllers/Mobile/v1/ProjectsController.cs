using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/projects")]
    [ApiController]
    public class ProjectsController(
        MainAppContext mainAppContext,
        UserManager<User> userManager,
        FileStorageService fileStorageService)
        : BaseController
    {
        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpPost]
        public async Task<IActionResult> PostProjectAsync([FromBody] ProjectInputDto? projectInputDto)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Unable to load user."));

            var userId = user.Id;
            if (projectInputDto != null)
            {
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

                var imageName = await fileStorageService.SaveFileAsync(projectInputDto.ImageFile);
                project.ImageName = imageName;
                
                await mainAppContext.Projects.AddAsync(project);
                await mainAppContext.SaveChangesAsync();
            }
            return StatusCode(StatusCodes.Status201Created, CreateSuccessResponse("Project added."));
        }

        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpGet("clientFeed")]
        public async Task<IActionResult> GetClientFeedAsync(
            [FromQuery] List<string>? quls, [FromQuery] int page = 0,
            [FromQuery] int pageSize = 8, [FromQuery] string? qur = default
        )
        {
            var imageUrl = $"{Request.Scheme}://{Request.Host}/projectImages";
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
                    CreationTime = StringOperations.GetTimeAgo(p.CreatedAt)
                })
                .ToListAsync();

            return Ok(CreateSuccessResponse(new
            {
                Total = count,
                Items = projects
            }));
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