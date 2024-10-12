using AonFreelancing.Models;
using Microsoft.AspNetCore.Mvc;

namespace AonFreelancing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private static List<Project>? projectList = new List<Project>();

        [HttpGet]
        public IActionResult GetAllProject()
        {
            if (projectList == null)
            {
                return NotFound("No projects found!");
            }
            return Ok(projectList);
        }

        [HttpGet("GetProjectBy{id}")]
        public IActionResult GetProjectById(int id)
        {
            if (projectList == null)
            {
                return NotFound("No projects found!");
            }
            Project? project = projectList.FirstOrDefault(p => p.Id == id);
            if (project == null)
            {
                return NotFound("No projects found!");
            }
            return Ok(project);
        }

        [HttpPost("CreateProject")]
        public IActionResult CreateProject([FromBody] Project project)
        {
            projectList ??= new List<Project>();

            projectList.Add(project);
            return CreatedAtAction("CreateProject", new { Id = project.Id }, projectList);
        }

        [HttpPut("UpdateProjectBy{id}")]
        public IActionResult UpdatePoject(int id, [FromBody] Project project)
        {
            if (projectList == null)
            {
                return NotFound("No projects found!");
            }

            Project? proj = projectList.FirstOrDefault(p => p.Id == id);
            if (proj == null)
            {
                return NotFound("No projects found!");
            }

            proj.Name = project.Name;
            proj.Description = project.Description;
            return Ok(proj);
        }

        [HttpDelete("DeleteProjectBy{id}")]
        public IActionResult DeleteProject (int id)
        {
            if (projectList == null)
            {
                return NotFound("No projects found!");
            }

            Project? proj = projectList.FirstOrDefault(p => p.Id == id);

            if (proj == null)
            {
                return NotFound("No projects found!");
            }

            projectList.Remove(proj);
            return NoContent();
        }
    }
}
