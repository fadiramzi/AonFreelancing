using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

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



        [Authorize(Roles = "Client")]
        [HttpPost("post-project")]
        public async Task<IActionResult> PostProject([FromBody] ProjectInputDTO projectInputDto)
        {
            var Username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var UserClient = await _userManager.FindByNameAsync(Username);
            
            Project project = new Project 
            {
                Title = projectInputDto.Title,
                Description = projectInputDto.Description,
                ClientId = UserClient.Id,
                QualificationName = projectInputDto.QualificationName,
                Budget = projectInputDto.Budget,
                PriceType = projectInputDto.PriceType,
                Duration = projectInputDto.Duration,
                CreatedAt = DateTime.Now,
            };

            await _mainAppContext.Projects.AddAsync(project);
            await _mainAppContext.SaveChangesAsync();
            return Ok(CreateSuccessResponse(project));
        }


        [HttpGet("feed")]
        public async Task<IActionResult> GetProjectsFeed(int pageSize = 10, int pageNumber = 1, string search_query = "", string qual = "")
        {
            var query = _mainAppContext.Projects.AsQueryable();

            // Search functionality
            if (!search_query.IsNullOrEmpty())
            {
                query = query.Where(p => p.Title.Contains(search_query) || p.Description.Contains(search_query));
            }

            // Filtering by qualifications
            if (!qual.IsNullOrEmpty())
            {
                var qualifications = qual.Split(',');
                query = query.Where(p => qualifications.Contains(p.QualificationName));
            }

            // Pagination functionality
            var projects = await query.Skip((pageNumber - 1) * pageSize)
                                           .Take(pageSize)
                                           .Select(p => new ProjectFeedDTO
                                           {
                                               Title = p.Title,
                                               Description = p.Description,
                                               Status = p.Status,
                                               Budget = p.Budget,
                                               Duration = p.Duration,
                                               Qualifications = p.QualificationName,
                                               CreationDate = p.CreatedAt
                                           })
                                           .ToListAsync();

            if (!projects.IsNullOrEmpty())
            {
                return Ok(CreateSuccessResponse(projects));
            }

            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "There are No projects !"));
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
