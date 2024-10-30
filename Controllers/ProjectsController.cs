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
        [HttpGet]
        public IActionResult GetAll()
        {
            // entryPoint of DB comuniction
            var data = _mainAppContext.Projects.ToList();
            return Ok(data);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] ProjectInputDTO project)
            
        {
            ApiResponse<object> apiResponse;

            Project p = new Project();
            p.Title = project.Title;
            p.Description = project.Description;
            p.ClientId = project.ClientId;

            await _mainAppContext.Projects.AddAsync(p);
            await _mainAppContext.SaveChangesAsync();
            apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = p
            };


            return Ok(apiResponse);
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreelancer(int id)
        {

            Project? fr = await _mainAppContext.Projects.FirstOrDefaultAsync(f => f.Id == id);

            if (fr == null)
            {
                return NotFound("The resoucre is not found!");
            }

            return Ok(fr);

        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Project f = _mainAppContext.Projects.FirstOrDefault(f => f.Id == id);
            if (f != null)
            {
                _mainAppContext.Remove(f);
                _mainAppContext.SaveChanges();
                return Ok("Deleted");

            }

            return NotFound();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] ProjectInputDTO project)
             
        {
            Project f = _mainAppContext.Projects.FirstOrDefault(f => f.Id == id);
            if (f != null)
            {
                
                f.Title = project.Title;
                f.Description = project.Description;
                f.ClientId = project.ClientId;

                _mainAppContext.SaveChanges();
                return Ok(f);

            }

            return NotFound();
        }
    }
}
