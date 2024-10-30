using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.DTOs.ClientDTO;
using AonFreelancing.Models.DTOs.ProjectDTOs;
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
                      Name = c.User.Name,
                      Username = c.User.Username
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
                     Name = c.User.Name,
                     Username = c.User.Username,
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
                        Name = c.User.Name,
                        Username = c.User.Username
                    }).SingleOrDefaultAsync();
            }
            else if (loadProjects == 1)
            {
                client = await _mainAppContext.Clients
                .Where(c => c.Id == Id)
                 .Include(c => c.Projects)
                 .Select(c => new ClientDTO
                 {
                    Id = c.User.Id,
                    CompanyName = c.CompanyName,
                    Name = c.User.Name,
                    Username = c.User.Username,
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

        //Creating a new client
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] ClientInputDTO clientDTO)
        {
            Client client = new();
            client.User = new User();
            client.CompanyName = clientDTO.CompanyName;
            client.User.Name = clientDTO.Name;
            client.User.Username = clientDTO.Username;
            client.User.Password = clientDTO.Password;
            await _mainAppContext.Clients.AddAsync(client);
            await _mainAppContext.SaveChangesAsync();
            return Ok(client);
        }

        //Removing a client
        [HttpDelete]
        public async Task<IActionResult> RemoveClientById([FromQuery] int Id)
        {
            Client? client = await _mainAppContext.Clients.FindAsync(Id);
            if (client is null)
                return NotFound($"Client {Id} is Not Found.");
            _mainAppContext.Clients.Remove(client);
            await _mainAppContext.SaveChangesAsync();

            return NoContent();
        }

        //Updating Client by Id
        [HttpPut]
        public async Task<IActionResult> UpdateClientById([FromQuery] int Id, [FromBody] ClientInputDTO clientDTO)
        {
            Client? client = await _mainAppContext.Clients.FindAsync(Id);
            if (client is null)
                return NotFound($"Client {Id} is Not Found.");

            // Updating client values
            client.User.Name = clientDTO.Name;
            client.User.Username = clientDTO.Username;
            client.CompanyName = clientDTO.CompanyName;
            client.User.Password = clientDTO.Password;

            await _mainAppContext.SaveChangesAsync();
            return Ok(client);
        }

    }
}
