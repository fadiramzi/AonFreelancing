using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

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
        public async Task<IActionResult> GetAll([FromQuery] string? Mode) {
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClientInputDTO clientDTO)
        {
            Client clint = new Client();
            clint.Name = clientDTO.Name;
            clint.CompanyName = clientDTO.CompanyName;
            clint.Username = clientDTO.Username;
            clint.Password = clientDTO.Password;
            await _mainAppContext.Clients.AddAsync(clint);
            await _mainAppContext.SaveChangesAsync();
            return CreatedAtAction("created", new { Id = clint.Id }, clint);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GitIdClient(int id)
        {
            Client? Cl = await _mainAppContext.Clients.FirstOrDefaultAsync(Cl => Cl.Id == id);

            if (Cl == null)
            {
                return NotFound("The resoucre is not found!");
            }

            return Ok(Cl);

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleatTheId(int id)
        {
            Client c = await _mainAppContext.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (c != null)
            {
                _mainAppContext.Remove(c);
                await _mainAppContext.SaveChangesAsync();
                return Ok("Deleted Done");

            }
            return NotFound();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateID(int id, [FromBody] Client clint)
        {
            Client c = await _mainAppContext.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (c != null)
            {
                c.Name = clint.Name;

                await _mainAppContext.SaveChangesAsync();
                return Ok(c);

            }
            return NotFound();

        }

        
       

    }
}
