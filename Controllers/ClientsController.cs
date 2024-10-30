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

        public ClientsController(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        [HttpGet]
        //get all client
        public async Task<IActionResult> GetAll([FromQuery] string? Mode)
        {
            var ClientList = new List<ClientDTO>();
            if (Mode == null || Mode == "basic")
            {
                ClientList = await _mainAppContext.Clients
                 .Include(c => c.Projects)
                  .Select(c => new ClientDTO
                  {
                      Id = c.Id,
                      CompanyName = c.CompanyName,
                      Name = c.Name,
                      Username = c.Username
                  })
                 .ToListAsync();
            }
            if (Mode == "r")
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

            return Ok(ClientList);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(int id)

        {
            Client? client = await _mainAppContext.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (client != null)
            {
              
                
                return Ok(client);
            }

          
            return NotFound();


        }
        //api/Client/
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClientInputDTO clientDTO)
        {
            Client? client = new Client
            {

                Name = clientDTO.Name,
                CompanyName = clientDTO.CompanyName,
                Username = clientDTO.Username,
                Password = clientDTO.Password,
            };
            await _mainAppContext.Clients.AddAsync(client);
            await _mainAppContext.SaveChangesAsync();
           


            return Ok(client);


        }
        [HttpPost("Register")]
        //api/Client/Register
        public async Task<IActionResult> Register([FromBody] ClientInputDTO clientDTO)
        {
            Client client = new Client();
            client.Name = clientDTO.Name;
            client.Username = clientDTO.Username;
            client.Password = clientDTO.Password;
            client.CompanyName = clientDTO.CompanyName;
            await _mainAppContext.Clients.AddAsync(client);
            await _mainAppContext.SaveChangesAsync();
            return Ok(client);


        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Client? c = await _mainAppContext.Clients.FirstOrDefaultAsync(f => f.Id == id);
            if (c != null)
            {
                _mainAppContext.Clients.Remove(c);
                await _mainAppContext.SaveChangesAsync();
                return Ok("Deleted");
            }
            return BadRequest();
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ClientInputDTO client)
        {
            Client? c = await _mainAppContext.Clients.FirstOrDefaultAsync(p => p.Id == id);

            if (c != null)
            {

                c.Name = client.Name;
                c.CompanyName = client.CompanyName;
                c.Username = client.Username;
                c.Password = client.Password;
            
                await _mainAppContext.SaveChangesAsync();
                return Ok(client);
            }

            return NotFound();


        }
    }
}
