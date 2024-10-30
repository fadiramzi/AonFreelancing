using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers
{
    [Route("api/Projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;
        public ProjectsController( MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        [HttpPost]
        public async Task <IActionResult >CreateProject([FromBody] ProjectInputDTO project)
        {

            Project p = new Project() {
                
                
                Title=project.Title,
            
            Description=project.Description,
            ClientId=project.ClientId,
            CreatedAt =project.CreatedAt,
            };

            await _mainAppContext.Projects.AddAsync(p);
            _mainAppContext.SaveChanges();

            return Ok(p);

          
        }

        [HttpGet("{id}")]
        public  async Task <IActionResult > GetProject(int id)
        {
            var project =  await _mainAppContext.Projects
                .Include(p=>p.Client)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (project != null) {

                return Ok(project); 
            }
            return NotFound();
        }
      

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProjectInputDTO project )
        {
            Project? p = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (p != null)
            {p.Title = project.Title;
                p.Description = project.Description;
                p.CreatedAt=project.CreatedAt;

                await _mainAppContext.SaveChangesAsync();


            }

            return NotFound();
        }

    }
}
