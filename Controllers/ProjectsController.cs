using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;
        public ProjectsController( MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjectsAsync()
        {
            var response = new ApiResponse<List<ProjectOutDTO>>();

            try
            {
                var projects = await _mainAppContext.Projects
                    .Include(p => p.Client)
                    .Select(p => new ProjectOutDTO
                    {
                        Id = p.Id,
                        ClientId = p.ClientId,
                        FreelancerId = p.FreelancerId,
                        CreatedAt = p.CreatedAt,
                        Description = p.Description,
                        Title = p.Title,
                    }).ToListAsync();

                response.IsSuccess = true;
                response.Results = projects;
                return Ok(response);
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "500",
                    Message = e.Message,
                };
                return StatusCode(500, response);
            }
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectAsync(int id)
        {
            var response = new ApiResponse<ProjectOutDTO>();

            try
            {
                var project = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
                
                if (project == null)
                {
                    response.IsSuccess = false;
                    response.Error = new Error()
                    {
                        Code = "404",
                        Message = "Project not found",
                    };
                    return NotFound(response);
                }

                var projectOutDto = new ProjectOutDTO
                {
                    Id = project.Id,
                    Title = project.Title,
                    Description = project.Description,
                    ClientId = project.ClientId,
                    CreatedAt = project.CreatedAt,
                    FreelancerId = project.FreelancerId,
                };
                
                response.IsSuccess = true;
                response.Results = projectOutDto;
                
                return Ok(response);
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "500",
                    Message = e.Message,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProjectAsync([FromBody] ProjectInputDTO projectInputDto)
        {
            var response = new ApiResponse<ProjectOutDTO>();
            if (!ModelState.IsValid)
            {
               response.IsSuccess = false;
               response.Error = new Error()
               {
                   Code = "422",
                   Message = "Invalid data",
               };
               
               return UnprocessableEntity(response);
            }
            
            try
            {
                var p = new Project
                {
                    Title = projectInputDto.Title,
                    Description = projectInputDto.Description,
                    ClientId = projectInputDto.ClientId,
                    FreelancerId = projectInputDto.FreelancerId,
                    CreatedAt = DateTime.Now
                };

                await _mainAppContext.Projects.AddAsync(p);
                await _mainAppContext.SaveChangesAsync();
                var projectOutDto = new ProjectOutDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    ClientId = p.ClientId,
                    FreelancerId = p.FreelancerId,
                    CreatedAt = p.CreatedAt
                };
                
                response.IsSuccess = true;
                response.Results = projectOutDto;
                
                return CreatedAtAction(nameof(GetProjectAsync), new { id = p.Id }, response);
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "500",
                    Message = e.Message,
                };
                return StatusCode(500, response);
            }
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProjectAsync(int id, [FromBody] ProjectInputDTO projectInputDto)
        {
            var response = new ApiResponse<ProjectOutDTO>();

            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "400",
                    Message = "Invalid data",
                };
            }
            
            var project = await _mainAppContext.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == id);

            try
            {
                if (project == null)
                {
                    response.IsSuccess = false;
                    response.Error = new Error()
                    {
                        Code = "404",
                        Message = "Project not found",
                    };
                }

                project!.Title = projectInputDto.Title;
                project.Description = projectInputDto.Description;
                project.ClientId = projectInputDto.ClientId;
                project.FreelancerId = projectInputDto.FreelancerId;

                await _mainAppContext.SaveChangesAsync();

                var updateProject = new ProjectOutDTO
                {
                    Id = project.Id,
                    Title = project.Title,
                    Description = project.Description,
                    ClientId = project.ClientId,
                    FreelancerId = project.FreelancerId,
                    CreatedAt = project.CreatedAt,
                };

                response.IsSuccess = true;
                response.Results = updateProject;

                return Ok(response);
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "500",
                    Message = e.Message,
                };
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectAsync(int id)
        {
            var response = new ApiResponse<string>();
            try
            {
                var project = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
                if (project == null)
                {
                    response.IsSuccess = false;
                    response.Error = new Error()
                    {
                        Code = "404",
                        Message = "Project not found",
                    };
                }
                _mainAppContext.Projects.Remove(project!);
                await _mainAppContext.SaveChangesAsync();
                
                response.IsSuccess = true;
                response.Results = "Project was successfully deleted.";

                return Ok(response);
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "500",
                    Message = e.Message,
                };
                return StatusCode(500 ,response);
            }
        }
    }
}
