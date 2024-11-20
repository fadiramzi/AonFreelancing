using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/projects")]
    [ApiController]
    public class ProjectsController(MainAppContext mainAppContext, UserManager<User> userManager) : BaseController
    {
<<<<<<< HEAD
        private readonly MainAppContext _mainAppContext;
        private readonly UserManager<User> _userManager;
        public ProjectsController(
            MainAppContext mainAppContext,
            UserManager<User> userManager
            )
        {
            _mainAppContext = mainAppContext;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectInputDTO project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<ProjectOutDTO>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error> { new Error { Code = "400", Message = "Invalid project data." } },
                    Message = "Validation failed"
                });
            }

            try
            {
                Project p = new Project
                {
                    Title = project.Title,
                    Description = project.Description,
                    ClientId = project.ClientId,
                    PriceType = project.PriceType,
                    Duration = project.Duration,
                    QualificationName = project.QualificationName,
                    Budget = project.Budget,
                    Status = "Available"
                };

                await _mainAppContext.Projects.AddAsync(p);
                await _mainAppContext.SaveChangesAsync();

                var projectOutDTO = new ProjectOutDTO
                {
                    Title = p.Title,
                    Description = p.Description,
                    CreatedAt = DateTime.UtcNow,
                    Status = p.Status,
                    Budget = p.Budget,
                    Duration = p.Duration,
                    PriceType = p.PriceType,
                    QualificationName = p.QualificationName
                };

                return Ok(new ApiResponse<ProjectOutDTO>
                {
                    IsSuccess = true,
                    Results = projectOutDTO,
                    Errors = null,
                    Message = "Project created successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ProjectOutDTO>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error> { new Error { Code = "500", Message = ex.Message } },
                    Message = "An error occurred while creating the project."
                });
            }
=======
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
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2
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
                CreationTime = StringOperations.GetTimeAgo(p.CreatedAt)
            })
            .ToListAsync();
           
            return Ok(CreateSuccessResponse(new { 
                Total=count,
                Items=projects
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
<<<<<<< HEAD

        [Authorize]
        [HttpGet("feed")]
        public async Task<IActionResult> GetProjectsFeed(
    int pageSize = 10,
    int pageNumber = 1,
    string search_query = "",
    string qual = "")
        {
            var query = _mainAppContext.Projects.AsQueryable();

            query = query.Where(p => p.Status == "Available");

            if (!string.IsNullOrEmpty(search_query))
            {
                query = query.Where(p => p.Title.Contains(search_query) || p.Description.Contains(search_query));
            }

            if (!string.IsNullOrEmpty(qual))
            {
                var qualifications = qual.Split(',');
                query = query.Where(p => qualifications.Contains(p.QualificationName));
            }

            var totalProjects = await query.CountAsync();

            var projects = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var projectDtos = projects.Select(p => new ProjectOutDTO
            {
                Title = p.Title,
                Description = p.Description,
                Status = p.Status,
                Budget = p.Budget,
                Duration = p.Duration,
                QualificationName = p.QualificationName,
                CreatedAt = DateTime.UtcNow,
                CreationTimeAgo = GetTimeAgo(p.CreatedAt)
            }).ToList();

            var response = new ApiResponse<IEnumerable<ProjectOutDTO>>
            {
                IsSuccess = true,
                Results = projectDtos,
                Message = "Projects feed loaded successfully",
                Errors = null
            };

            Response.Headers.Add("X-Total-Count", totalProjects.ToString());
            Response.Headers.Add("X-Total-Pages", Math.Ceiling(totalProjects / (double)pageSize).ToString());

            return Ok(response);
        }

        private string GetTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.UtcNow - createdAt;
            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute(s) ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour(s) ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} day(s) ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} month(s) ago";
            return $"{(int)(timeSpan.TotalDays / 365)} year(s) ago";
        }

=======
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2
    }
}