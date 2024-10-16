using AonFreelancing.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AonFreelancing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private static List<Project> ProjectList = new List<Project>();
        
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(ProjectList);
        }

        [HttpPost]
        public IActionResult Create(Project project)
        {
            ProjectList.Add(project);
            return CreatedAtAction(nameof(Create), new { Id = project.Id }, project);
        }

        [HttpGet("{id}")]
        public IActionResult GetProject(int id)
        {
            var Project = ProjectList.FirstOrDefault(p => p.Id == id);

            if (Project == null)
            {
                return NotFound("The resoucre is not found!");
            }

            return Ok(Project);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var Project = ProjectList.FirstOrDefault(p => p.Id == id);
            if (Project != null)
            {
                ProjectList.Remove(Project);
                return NoContent();
            }

            return NotFound();
        }
    }
}
