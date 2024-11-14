using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/projects")]
    [ApiController]
    public class ProjectsController : BaseController
    {
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
        }


        //[HttpGet("{id}")]
        //public IActionResult GetProject(int id)
        //{
        //    var project = _mainAppContext.Projects
        //        .Include(p => p.Client)
        //        .FirstOrDefault(p => p.Id == id);

        //    return Ok(CreateSuccessResponse(project));

        //}

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

    }
}
