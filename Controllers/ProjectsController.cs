using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        [HttpGet]

        public async Task<IActionResult> GetAll()
        {
            var data = await _mainAppContext.Projects.ToListAsync();
            return Ok(data);
        }

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetProject(int id) 
        //{
        //    Project? pr = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
        //    if (pr == null)
        //    {
        //        return NotFound("THE RESOURS NO FOUND!");


        //    }
        //    return Ok(pr);
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateID(int id , [FromBody] Project project)
        {
            Project p = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (p != null)
            {
                p = project;
                _mainAppContext.SaveChanges();
                return Ok(p);

            }
            return NotFound();


        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DelateID(int id)
        {
            Project p = await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (p != null)
            {
                _mainAppContext.Remove(p);
                await _mainAppContext.SaveChangesAsync();
                return Ok("Deleted Done");
            }
            return NotFound();
        }






    }
}
