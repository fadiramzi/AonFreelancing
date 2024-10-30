using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AonFreelancing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;
        public ProjectsController(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            List<ProjectOutputDTO> projectOutputDTOs = await _mainAppContext.Projects.Include(p => p.Client)
                                                                                       .Select(p => new ProjectOutputDTO(p))
                                                                                       .ToListAsync();
            ApiResponse<object> apiResponse = new ApiResponse<object>()
            {
                IsSuccess = true,
                Results = projectOutputDTOs
            };
            return Ok(apiResponse);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectAsync([FromRoute] int id,
            [FromQuery][BindRequired][Range(0, 1, ErrorMessage = "loadFreelancers must be either 0 or 1")] int loadFreelancers)
        {
            Project? project;
            if (loadFreelancers == 0)
                project = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
            else
                project = await _mainAppContext.Projects.Include(p => p.Freelancers).FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound(new ApiResponse<object> { Error = new Error { Message = "Resource not found", Code = "404" } });

            ApiResponse<object> apiResponse = new ApiResponse<object>()
            {
                IsSuccess = true,
                Results = new ProjectOutputDTO(project) { Freelancers = project.Freelancers.Select(f => new FreelancerOutputDTO(f)) }
            };
            return Ok(apiResponse);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ProjectInputDTO projectInputDTO)
        {
            Project project = new Project(projectInputDTO);
            try
            {
                await _mainAppContext.Projects.AddAsync(project);
                await _mainAppContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>() { Error = new Error() { Message = ex.Message, Code = "500" } });
            }
            ApiResponse<object> apiResponse = new ApiResponse<object>()
            {
                IsSuccess = true,
                Results = new ProjectOutputDTO(project)
            };

            return CreatedAtAction(nameof(GetProjectAsync), new { id = project.Id, loadFreelancers = 0 }, apiResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] ProjectInputDTO projectInputDTO)
        {
            Project? project = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                return NotFound("The resource is not found");

            project.Title = projectInputDTO.Title;
            project.Description = projectInputDTO.Description ?? project.Description;

            await _mainAppContext.SaveChangesAsync();
            ApiResponse<object> apiResponse = new ApiResponse<object>()
            {
                IsSuccess = true,
                Results = new ProjectOutputDTO(project)
            };

            return Ok(apiResponse);
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignFreelancerToProjectAsync([FromRoute] int id, [FromQuery] int freelancerId)
        {
            Project? project = await _mainAppContext.Projects.Include(p => p.Freelancers).FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                return NotFound(new ApiResponse<object> { Error = new Error { Message = "Resource not found", Code = "404" } });

            Freelancer? freelancer = await _mainAppContext.Freelancers.Include(f => f.Projects).FirstOrDefaultAsync(f => f.Id == freelancerId);
            if (freelancer == null)
                return UnprocessableEntity(new ApiResponse<object> { Error = new Error { Message = "The freelancerId is not valid, use an existing freelancerId", Code = "422" } });

            project.Freelancers.Add(freelancer);
            freelancer.Projects.Add(project);

            await _mainAppContext.SaveChangesAsync();
            ApiResponse<object> apiResponse = new ApiResponse<object>()
            {
                IsSuccess = true,
                Results = new ProjectOutputDTO(project) { Freelancers = project.Freelancers.Select(f => new FreelancerOutputDTO(f)) }
            };

            return Ok(apiResponse);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            Project? project = await _mainAppContext.Projects.Include(p => p.Freelancers).FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound(new ApiResponse<object> { Error = new Error { Message = "Resource not found", Code = "404" } });

            project.Freelancers = [];//break the relationship between the project and its freelancers

            _mainAppContext.Remove(project);
            await _mainAppContext.SaveChangesAsync();
            ApiResponse<object> apiResponse = new ApiResponse<object>()
            {
                IsSuccess = true,
                Results = "Deleted"
            };
            return Ok(apiResponse);
        }
    }
}
