using AonFreelancing.Models;
using Microsoft.AspNetCore.Mvc;

namespace AonFreelancing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private static List<Project>? _projectList = [];

        [HttpGet]
        public IActionResult GetAllProject()
        {
            if (_projectList == null)
            {
                return NotFound("No projects found!");
            }
            return Ok(_projectList);
        }

        [HttpGet("GetProjectBy{id}")]
        public IActionResult GetProjectById(int id)
        {
            if (_projectList == null)
            {
                return NotFound("No projects found!");
            }
            var project = _projectList.FirstOrDefault(p => p.Id == id);
            if (project == null)
            {
                return NotFound("No projects found!");
            }
            return Ok(project);
        }

        [HttpPost("CreateProject")]
        public IActionResult CreateProject([FromBody] Project? project)
        {
            _projectList ??= [];

            if (project == null)
            {
                return NotFound("No projects found!");
            }

            if (_projectList.Any(p => p.Id == project.Id))
            {
                return Conflict("Project already exists!");
            }
            
            _projectList.Add(project);
            return CreatedAtAction("CreateProject", new { project.Id }, _projectList);
        }

        [HttpPut("UpdateProjectBy{id}")]
        public IActionResult UpdateProject(int id, [FromBody] Project project)
        {
            if (_projectList == null)
            {
                return NotFound("No projects found!");
            }

            var proj = _projectList.FirstOrDefault(p => p.Id == id);
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
            if (_projectList == null)
            {
                return NotFound("No projects found!");
            }

            var proj = _projectList.FirstOrDefault(p => p.Id == id);

            if (proj == null)
            {
                return NotFound("No projects found!");
            }

            _projectList.Remove(proj);
            return NoContent();
        }
    }
}
