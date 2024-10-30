using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs.ProjectDTOs;
using AonFreelancing.Models.DTOs.ResponseDTOs;
using Microsoft.AspNetCore.Http;
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

        // Create Project
        [HttpPost]
        public async Task <IActionResult> CreateProject([FromBody] ProjectInputDTO projectDTO)
        {
            ApiResponseDTO<object> apiResponseDTO;

            Project project = new()
            {
                Title = projectDTO.Title,
                Description = projectDTO.Description,
                ClientId = projectDTO.ClientId,
                FreelancerId = projectDTO.FreelancerId,
            };

            await _mainAppContext.Projects.AddAsync(project);
            await _mainAppContext.SaveChangesAsync();

            apiResponseDTO =  new()
            {
                IsSuccess = true,
                Results = new ProjectOutDTO()
                {
                    Id = project.Id,
                    ClientId = project.ClientId,
                    FreelancerId = project.FreelancerId,
                    Title = project.Title,
                    Description = project.Description,
                }
            };

            return Ok(apiResponseDTO);
        }

        // Get all projects
        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            List<ProjectOutDTO> projects = await _mainAppContext.Projects
                .Select(project => new ProjectOutDTO() 
                {
                    Title = project.Title,
                    Description = project.Description,
                    ClientId = project.ClientId,
                    FreelancerId = project.FreelancerId,
                    Id = project.Id
                })
                .ToListAsync();
            ApiResponseDTO<object> apiResponseDTO = new()
            {
                IsSuccess= true,
                Results = projects,
            };
            return Ok(apiResponseDTO);
        }

        // Get Project by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            ApiResponseDTO<object> apiResponseDTO;
            Project? project = await _mainAppContext.Projects.FindAsync(id);
            
            if (project == null)
            {
                apiResponseDTO = new()
                {
                    IsSuccess = false,
                    Error = new Error()
                    { Code = 404, Message = $"Project { id } is not found."}
                };
                return NotFound(apiResponseDTO);
            }
            ProjectOutDTO projectOutDTO = new()
            {
                Title = project.Title,
                Description = project.Description,
                ClientId = project.ClientId,
                FreelancerId = project.FreelancerId,
                Id = project.Id
            };
            apiResponseDTO = new()
            {
                IsSuccess = true,
                Results = projectOutDTO,
            };
            return Ok(apiResponseDTO);
        
        }

        // Updating Project data
        [HttpPut]
        public async Task<IActionResult> UpdateProjectById(int id, [FromBody] ProjectInputDTO projectDTO)
        {
            ApiResponseDTO<object> apiResponseDTO;
            Project? project = await _mainAppContext.Projects.FindAsync(id);
            if (project == null)
            {
                apiResponseDTO = new()
                {
                    IsSuccess = false,
                    Error = new Error()
                    { Code = 404, Message = $"Project {id} is not found."}
                };
                return NotFound(apiResponseDTO);
            }
            project.Title = projectDTO.Title;
            project.Description = projectDTO.Description;
            project.ClientId = projectDTO.ClientId;
            project.FreelancerId = projectDTO.FreelancerId;
            await _mainAppContext.SaveChangesAsync();

            ProjectOutDTO projectOutDTO = new()
            {
                Title = project.Title,
                Description = project.Description,
                ClientId = project.ClientId,
                FreelancerId = project.FreelancerId,
                Id = project.Id
            };
            apiResponseDTO = new()
            {
                IsSuccess = true,
                Results = projectOutDTO,
            };
            return Ok(apiResponseDTO);
        }

        // Remove project by Id
        [HttpDelete]
        public async Task<IActionResult> RemoveProjectById(int id)
        {
            ApiResponseDTO<object> apiResponseDTO;
            Project? project = await _mainAppContext.Projects.FindAsync(id);
            if (project == null)
            {
                apiResponseDTO = new()
                {
                    IsSuccess = false,
                    Error = new Error()
                    { Code = 404, Message = $"Project {id} is not found." }
                };
                return NotFound(apiResponseDTO);
            }
            _mainAppContext.Remove(project);
            await _mainAppContext.SaveChangesAsync();
            ProjectOutDTO projectOutDTO = new()
            {
                Title = project.Title,
                Description = project.Description,
                ClientId = project.ClientId,
                Id = project.Id
            };
            apiResponseDTO = new()
            {
                IsSuccess = true,
                Results = projectOutDTO,
            };
            return Ok(apiResponseDTO);
        }
    }
}
