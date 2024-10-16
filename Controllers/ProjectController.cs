using AonFreelancing.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AonFreelancing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private static readonly List<Project> _projectList = new List<Project>();
        
        [HttpGet]
        public IActionResult GetAllProjects()
        {
            return Ok(_projectList);
        }

        [HttpPost]
        public IActionResult CreateProject(Project project)
        {
            _projectList.Add(project);
            return CreatedAtAction(nameof(CreateProject), new { Id = project.Id }, project);
        }

        [HttpGet("{id}")]
        public IActionResult GetProject(int id)
        {
            var project = _projectList.FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                return NotFound("The resoucre is not found!");
            }

            return Ok(project);
        }

        [HttpDelete("{id}")]
        public IActionResult RemoveProject(int id)
        {
            var project = _projectList.FirstOrDefault(p => p.Id == id);
            if (project != null)
            {
                _projectList.Remove(project);
                return NoContent();
            }

            return NotFound();
        }
    }
}
