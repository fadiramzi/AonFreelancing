using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;

namespace AonFreelancing.Controllers
{
    [Route("api/v1/clients")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;

        public ClientsController(MainAppContext mainAppContext) { 
            _mainAppContext = mainAppContext;
        }

        //api/Client Get
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? Mode) {
            var ClientList = new List<ClientDTO>();
            if(Mode == null || Mode == "basic")
            {
                // I/O with DataBase
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
            if(Mode == "r")
            {
                // I/O with DataBase
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

        //api/Client Post
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClientInputDTO clientDTO)
        {
            Client client = new Client();
            client.Name = clientDTO.Name;
            client.CompanyName = clientDTO.CompanyName;
            client.Username = clientDTO.Username;
            client.Password = clientDTO.Password;

            // I/O with DataBase
            await _mainAppContext.Clients.AddAsync(client);
            await _mainAppContext.SaveChangesAsync();

            return CreatedAtAction("Create", new { Id = client.Id }, client);
        }

        //api/Client/{id} Get
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(int id)
        {
            // Searching in DataBase
            Client? client = await _mainAppContext.Clients.Include(c => c.Projects).FirstOrDefaultAsync(cl => cl.Id == id);

            if (client == null)
            {
                return NotFound("The Client is not found!");
            }

            return Ok(client);
        }

        //api/Client/{id} Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            // Searching in DataBase
            Client? client = await _mainAppContext.Clients
                                    .Include(c => c.Projects) // delete client will delete all their projects 
                                    .FirstOrDefaultAsync(cl => cl.Id == id);

            if(client == null)
            {
                return NotFound("The Client is not found!");
            }

            // I/O with DataBase
            _mainAppContext.Remove(client);
            await _mainAppContext.SaveChangesAsync();
            return Ok("The Client is deleted !");
        }

        //api/Client/{id} Update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] ClientInputDTO clientDTO)
        {
            // Searching in DataBase
            Client? client = await _mainAppContext.Clients.FirstOrDefaultAsync(cl => cl.Id == id);

            if (client == null)
            {
                return NotFound("The Client is not found!");
            }

            // I/O with DataBase
            client.Name = clientDTO.Name;
            client.CompanyName = clientDTO.CompanyName;
            client.Username = clientDTO.Username;
            client.Password = clientDTO.Password;

            await _mainAppContext.SaveChangesAsync();
            return Ok(client);

        }
    }
}
