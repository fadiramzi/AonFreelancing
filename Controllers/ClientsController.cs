using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers
{
    [Route("api/clients")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;

        public ClientsController(MainAppContext mainAppContext) { 
            _mainAppContext = mainAppContext;
        }

        // Get loadProjects 0: Get clients without loadding there projects, it's the defalut loadProjects.
        // Get loadProjects 1: Get clients with there projects.

        //Get all clients
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? loadProjects) {
            var ClientList = new List<ClientDTO>();
            if(loadProjects == null || loadProjects == 0)
            {
                ClientList = await _mainAppContext.Clients
                  .Select(c => new ClientDTO
                  {
                      Id = c.Id,
                      CompanyName = c.CompanyName,
                      Name = c.Name,
                      Username = c.Username
                  })
                 .ToListAsync();
            }
            else if(loadProjects == 1)
            {
                ClientList = await _mainAppContext.Clients
                .Include(c => c.Projects)
                 .Select(c => new ClientDTO
                 {
                     Id = c.Id,
                     CompanyName = c.CompanyName,
                     Name = c.Name,
                     Username = c.Username,
                     Projects = c.Projects.Select(p => new ProjectOutDTO
                     {
                         Id = p.Id,
                         Title = p.Title,
                         Description = p.Description,

                     })
                 })
                .ToListAsync();
            }
            else
                return BadRequest($"{loadProjects} is not a valid value.");
            return Ok(ClientList);
        }
        //Get client by Id
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetClientById(int Id, [FromQuery] int? loadProjects)
        {
            var client = new ClientDTO();
            if (loadProjects == null || loadProjects == 0)
            {
                client = await _mainAppContext.Clients
                    .Where(c => c.Id == Id)
                    .Select(c => new ClientDTO
                    {
                        Id = c.Id,
                        CompanyName = c.CompanyName,
                        Name = c.Name,
                        Username = c.Username
                    }).SingleOrDefaultAsync();
            }
            else if (loadProjects == 1)
            {
                client = await _mainAppContext.Clients
                .Where(c => c.Id == Id)
                 .Include(c => c.Projects)
                 .Select(c => new ClientDTO
                 {
                    Id = c.Id,
                    CompanyName = c.CompanyName,
                    Name = c.Name,
                    Username = c.Username,
                    Projects = c.Projects.Select(p => new ProjectOutDTO
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                    })
                 }).SingleOrDefaultAsync(); 
            }
            else
                return BadRequest($"{loadProjects} is not a valid value.");

            if (client is not null)
                return Ok(client);
            return NotFound($"No client with {Id} ID.");
        }

        [HttpPost]
        public IActionResult Create([FromBody] ClientInputDTO clientDTO)
        {
            return Ok("created");
        }
    }
}
