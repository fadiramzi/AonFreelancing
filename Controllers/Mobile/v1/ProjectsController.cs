using System.Security.Claims;
using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/projects")]
    [ApiController]
    public class ProjectsController(MainAppContext mainAppContext) : BaseController
    {
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectInputDto project)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var p = new Project
            {
                Title = project.Title,
                Description = project.Description,
            };

            await mainAppContext.Projects.AddAsync(p);
            await mainAppContext.SaveChangesAsync();
            
            return Ok(CreateSuccessResponse(p));
        }


        //[HttpGet("{id}")]
        //public IActionResult GetProject(int id)
        //{
        //    var project = _mainAppContext.Projects
        //        .Include(p => p.Client)
        //        .FirstOrDefault(p => p.Id == id);

        //    return Ok(CreateSuccessResponse(project));

        //}


    }
}
