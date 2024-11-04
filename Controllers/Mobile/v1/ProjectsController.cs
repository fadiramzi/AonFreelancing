using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Route("api/mobile/v1/projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;
        public ProjectsController(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        [HttpPost]
        public IActionResult CreateProject([FromBody] ProjectInputDTO project)
        {
            Project p = new Project();
            p.Title = project.Title;
            p.Description = project.Description;
            p.ClientId = project.ClientId;

            _mainAppContext.Projects.Add(p);
            _mainAppContext.SaveChanges();
            return Ok(p);
        }


        [HttpGet("{id}")]
        public IActionResult GetProject(int id)
        {
            var project = _mainAppContext.Projects
                .Include(p => p.Client)
                .FirstOrDefault(p => p.Id == id);

            return Ok(project);

        }


    }
}
