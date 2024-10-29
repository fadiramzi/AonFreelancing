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
        public ProjectsController(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
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
        public async Task<IActionResult> GetProject([FromRoute]int id)
        {
           Project? project = await _mainAppContext.Projects.Include(p => p.Client).FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                return NotFound("The resource is not found");

            ApiResponse<object> apiResponse = new ApiResponse<object>()
            {
                IsSuccess = true,
                Results = new ProjectOutputDTO(project) 
            };
            return Ok(apiResponse);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProjectInputDTO projectInputDTO)
        {
            Project project = new Project(projectInputDTO);

            await _mainAppContext.Projects.AddAsync(project);
            await _mainAppContext.SaveChangesAsync();

            ApiResponse<object> apiResponse = new ApiResponse<object>()
            {
                IsSuccess = true,
                Results = new ProjectOutputDTO(project)
            };

            return CreatedAtAction(nameof(GetProject), new { Id = project.Id }, apiResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ProjectInputDTO projectInputDTO)
        {
            Project? project = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                return NotFound("The resource is not found");

            project.Title = projectInputDTO.Title ;
            project.Description = projectInputDTO.Description ?? project.Description;

            await _mainAppContext.SaveChangesAsync();
            ApiResponse<object> apiResponse = new ApiResponse<object>()
            {
                IsSuccess = true,
                Results = new ProjectOutputDTO(project)
            };

            return Ok(apiResponse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult>Delete(int id)
        {
            Project?project = await _mainAppContext.Projects.FirstOrDefaultAsync(p=>p.Id == id);
            
            if (project == null)
                return NotFound("The resource is not found");

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
