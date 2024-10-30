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

        // Read all Projects
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // entryPoint of DB comuniction
            var data = await _mainAppContext.Projects.ToListAsync();
            return Ok(data);
        }

        // Create new project
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectInputDTO project)
        {
            // we can use Api Response for validation But I don't want to use it here
            Project p = new Project();
            p.Title = project.Title;
            p.Description = project.Description;
            p.ClientId = project.ClientId;
            p.FreelancerId = project.FreelancerId;

            await _mainAppContext.Projects.AddAsync(p);
            await _mainAppContext.SaveChangesAsync();
            return Ok(p);
        }

        // Get project by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        { 
            Project? p = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (p == null)
            {
                return NotFound("The resoucre is not found!");
            }

            return Ok(p);

        }

        // Delete Projects by id 
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Project? p = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (p != null)
            {
                _mainAppContext.Remove(p); // Entity for deletion.
                await _mainAppContext.SaveChangesAsync(); // saving the changes to the database
                return Ok("Deleted");

            }

            return NotFound();
        }

        // update Project by id 
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProjectInputDTO ProjectDTO)
        {
            Project? p = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (p != null)
            {
                p.Title = ProjectDTO.Title; // update title
                p.Description = ProjectDTO.Description; // update description
                p.ClientId = ProjectDTO.ClientId; // update ClientId
                p.FreelancerId = ProjectDTO.FreelancerId; // update FreelancerId

                await _mainAppContext.SaveChangesAsync(); // saving the changes to the database
                return Ok(p);

            }

            return NotFound();
        }

    }
}