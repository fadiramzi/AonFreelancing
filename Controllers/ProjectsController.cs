using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers
{
    [Route("api/v1/projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;
        public ProjectsController( MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        //api/Projects Get
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // I/O with DataBase
            var projects = await _mainAppContext.Projects
                                .Include(p => p.Client)
                                .Include(p => p.Freelancer)
                                .ToListAsync();
            return Ok(projects);
        }

        //api/Projects Post
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectInputDTO projectDto)
        {
            Project project = new Project();
            project.Title = projectDto.Title;
            project.Description = projectDto.Description;
            project.ClientId = projectDto.ClientId;

            // I/O with DataBase
            await _mainAppContext.Projects.AddAsync(project);
            await _mainAppContext.SaveChangesAsync();

            return CreatedAtAction("Create", new { Id = project.Id }, project);
        }

        //api/Projects/{id}  Get
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            

            // Searching in DataBase
            Project? project = await _mainAppContext.Projects
                                          .Include(p => p.Client)
                                          .FirstOrDefaultAsync(pr => pr.Id == id);

            if (project == null)
            {
                return NotFound("The Project is not found!");
            }

            return Ok(project);
        }

        //api/Project/{id} Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            // Searching in DataBase
            Project? project = await _mainAppContext.Projects.FirstOrDefaultAsync(pr => pr.Id == id);

            if (project == null)
            {
                return NotFound("The Project is not found!");
            }

            // I/O with DataBase
            _mainAppContext.Remove(project);
            await _mainAppContext.SaveChangesAsync();
            return Ok("The Project is deleted !");
        }

        //api/Project/{id} Update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] Project updatedProject)
        {
            // Searching in DataBase
            Project? currProject = await _mainAppContext.Projects.FirstOrDefaultAsync(pr => pr.Id == id);

            if (currProject == null)
            {
                return NotFound("The Project is not found!");
            }

            // I/O with DataBase
            currProject = updatedProject;
            await _mainAppContext.SaveChangesAsync();
            return Ok(currProject);

        }
    }
}
