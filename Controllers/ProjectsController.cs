using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
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
            Project project = new()
            {
                Title = projectDTO.Title,
                Description = projectDTO.Description,
                ClientId = projectDTO.ClientId,
            };

            await _mainAppContext.Projects.AddAsync(project);
            await _mainAppContext.SaveChangesAsync();
            return Ok(project);
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
                    Id = project.Id
                })
                .ToListAsync();
            return Ok(projects);
        }

        // Get Project by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            Project? project = await _mainAppContext.Projects.FindAsync(id);
            
            if (project == null) 
                return NotFound();
            ProjectOutDTO projectOutDTO = new()
            {
                Title = project.Title,
                Description = project.Description,
                ClientId = project.ClientId,
            };

            return Ok(project);
        
        }

        // Updating Project data
        [HttpPut]
        public async Task<IActionResult> UpdateProjectById(int id, [FromBody] ProjectInputDTO projectDTO)
        {
            Project? project = await _mainAppContext.Projects.FindAsync(id);
            if (project == null)
                return NotFound($"Project { id } is not found.");
            project.Title = projectDTO.Title;
            project.Description = projectDTO.Description;
            project.ClientId = projectDTO.ClientId;
            await _mainAppContext.SaveChangesAsync();
            return Ok();
        }
    }
}
