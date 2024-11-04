using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Route("api/mobile/v1/clients")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;
        private readonly UserManager<User> _userManager;
        public ClientsController(
            MainAppContext mainAppContext,
            UserManager<User> userManager
            )
        {
            _mainAppContext = mainAppContext;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] string? Mode)
        {
            var ClientList = new List<ClientDTO>();
            if (Mode == null || Mode == "basic")
            {
                ClientList = await _userManager.Users.OfType<Client>()
                 .Include(c => c.Projects)
                  .Select(c => new ClientDTO
                  {
                      Id = c.Id,
                      CompanyName = c.CompanyName,
                      Name = c.Name,
                      Username = c.UserName
                  })
                 .ToListAsync();
            }
            if (Mode == "r")
            {
                ClientList = await _userManager.Users.OfType<Client>()
                .Include(c => c.Projects)
                 .Select(c => new ClientDTO
                 {
                     Id = c.Id,
                     CompanyName = c.CompanyName,
                     Name = c.Name,
                     Username = c.UserName,
                     Projects = c.Projects.Select(p => new ProjectOutDTO
                     {
                         Id = p.Id,
                         Title = p.Title,
                         Description = p.Description,

                     })
                 })
                .ToListAsync();
            }

            return Ok(new ApiResponse<IEnumerable<ClientDTO>>()
            {
                IsSuccess = true,
                Results = ClientList,
                Errors = []
            });
        }


    }
}
