using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/projects")]
    [ApiController]
    public class ProjectsController : BaseController
    {
        private readonly MainAppContext _mainAppContext;
        private readonly UserManager<User> _userManager;
        public ProjectsController(
            MainAppContext mainAppContext,
            UserManager<User> userManager
            )
        {
            _mainAppContext = mainAppContext;
            _userManager = userManager;
        }



        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectInputDTO project)
        {
            var Username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var UserClient = await _userManager.FindByNameAsync(Username);

            Project p = new Project();
            p.Title = project.Title;
            p.Description = project.Description;
            p.ClientId = UserClient.Id;
            p.QualificationName = project.QualificationName;
            p.Budget = project.Budget;
            p.PriceType = project.PriceType;
            p.Duration = project.Duration;

            _mainAppContext.Projects.Add(p);
            _mainAppContext.SaveChanges();
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
