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


        [HttpGet("{id}")]
        public IActionResult GetProject(int id)
        {
            var project = _mainAppContext.Projects
                .Include(p=>p.Client)
                .FirstOrDefault(p => p.Id == id);

            return Ok(project);
            
        }


    }
}
