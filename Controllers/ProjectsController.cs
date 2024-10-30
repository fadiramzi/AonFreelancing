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

        // get all projects
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // entryPoint of DB comuniction
            var data = await _mainAppContext.Projects.ToListAsync();
            return Ok(data);
        }

        //retrieve a single project by his id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var Project = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (Project == null)
            {
                return NotFound();
            }
           
            return Ok(Project);
        }

        // Add new project
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectInputDTO projectInputDTO)
        {

            Project p = new Project();
            p.Title = projectInputDTO.Title;
            p.Description = projectInputDTO.Description;
            p.ClientId = projectInputDTO.ClientId;
            p.FreelancerId = projectInputDTO.FreelancerId;
          
            await _mainAppContext.Projects.AddAsync(p);
            await _mainAppContext.SaveChangesAsync();
            return Ok(p);
        }

        // Delete exicting project
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            Project? p = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (p != null)
            {
                _mainAppContext.Remove(p);
                await _mainAppContext.SaveChangesAsync();
                return Ok("Deleted");

            }

            return NotFound();
        }

        // Update existing project
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectInputDTO projectInputDTO)
        {
            Project? p = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
            
            if (p != null)
            {
                p.ClientId = projectInputDTO.ClientId;
                p.FreelancerId= projectInputDTO.FreelancerId;
                p.Title= projectInputDTO.Title;
                p.Description = projectInputDTO.Description;
                
                await _mainAppContext.SaveChangesAsync();
                return Ok(p);

            }

            return NotFound();
        }





    }
}
