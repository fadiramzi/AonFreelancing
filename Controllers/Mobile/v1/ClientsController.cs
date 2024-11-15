using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers.Mobile.v1
{
    // [Authorize]
    [Route("api/mobile/v1/clients")]
    [ApiController]
    public class ClientsController : BaseController
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

        [Route("CreateProject")]
        [HttpPost]
        public async Task<IActionResult> CreateProject(ProjectInputDTO projectDTO, long clientId)
        {
            var client = _mainAppContext.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), $"{clientId} isn't associated with any user."));
            var project = new Project()
            {
                Title = projectDTO.Title,
                Description = projectDTO.Description,
                Duration = projectDTO.Duration,
                PriceType = projectDTO.PriceType,
                Budget = projectDTO.Budget,
                CreatedAt = projectDTO.CreatedAt,
                QualificationName = projectDTO.QualificationName,
                Status = projectDTO.Status,
                ClientId = clientId,
            };
            await _mainAppContext.Projects.AddAsync(project);
            await _mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse<ProjectResponseDTO>(new ProjectResponseDTO()
            {
                Title = projectDTO.Title,
                Description = projectDTO.Description,
                Duration = projectDTO.Duration,
                PriceType = projectDTO.PriceType,
                Budget = projectDTO.Budget,
                CreatedAt = projectDTO.CreatedAt,
                QualificationName = projectDTO.QualificationName,
                Status = projectDTO.Status,
                ClientId = clientId,
            }));
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAllAsync([FromQuery] string? Mode)
        //{
        //    var ClientList = new List<ClientDTO>();
        //    if (Mode == null || Mode == "basic")
        //    {
        //        ClientList = await _userManager.Users.OfType<Client>()
        //         .Include(c => c.Projects)
        //          .Select(c => new ClientDTO
        //          {
        //              Id = c.Id,
        //              CompanyName = c.CompanyName,
        //              Name = c.Name,
        //              Username = c.UserName
        //          })
        //         .ToListAsync();
        //    }
        //    if (Mode == "r")
        //    {
        //        ClientList = await _userManager.Users.OfType<Client>()
        //        .Include(c => c.Projects)
        //         .Select(c => new ClientDTO
        //         {
        //             Id = c.Id,
        //             CompanyName = c.CompanyName,
        //             Name = c.Name,
        //             Username = c.UserName,
        //             Projects = c.Projects.Select(p => new ProjectOutDTO
        //             {
        //                 Id = p.Id,
        //                 Title = p.Title,
        //                 Description = p.Description,

        //             })
        //         })
        //        .ToListAsync();
        //    }

        //    return Ok(CreateSuccessResponse(ClientList));
        //}


    }
}
