using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static AonFreelancing.Models.DTOs.ClientDTO;

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

        // get all clients
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // entryPoint of DB comuniction
            var data = await _mainAppContext.Clients.ToListAsync();
            return Ok(data);
        }

        //retrieve a single client by his id
        [HttpGet("BasicGetById")]
        public async Task<IActionResult> GetClientById(int id)
        {
            ClientBasicDTO clientBasicDTO = new ClientBasicDTO();
            var Client = await _mainAppContext.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (Client == null)
            {
                return NotFound();
            }
            clientBasicDTO.Id = Client.Id;
            clientBasicDTO.Name = Client.Name;
            clientBasicDTO.Username = Client.Username;
            clientBasicDTO.CompanyName = Client.CompanyName;

            return Ok(clientBasicDTO);
        }

        
        // Retrieve all clients info only or with their projcts based on Query Parameter 
        [HttpGet("GetFromQuery")]
        public async Task<IActionResult> GetAllClients([FromQuery] string? Mode)
        {
            var ClientList = new List<ClientOutDTO>();
            if (Mode == null || Mode == "basic")
            {
                ClientList = await _mainAppContext.Clients
                 .Include(c => c.Projects)
                  .Select(c => new ClientOutDTO
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
                 .Select(c => new ClientOutDTO
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

        //// Add new client
    
        //api/clients
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] ClientInputDTO clientInputDTO)
        {

            Client c = new Client();
            c.Name = clientInputDTO.Name;
            c.Username = clientInputDTO.Username;
            c.Password = clientInputDTO.Password;
            c.CompanyName = clientInputDTO.CompanyName;
            await _mainAppContext.Clients.AddAsync(c);
            await _mainAppContext.SaveChangesAsync();
            return Ok(c);
        }

        //api/clients/Register
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterClient([FromBody] ClientInputDTO clientInputDTO)
        {
            ApiResponse<object> apiResponse;

            Client c = new Client();
            c.Name = clientInputDTO.Name;
            c.Username = clientInputDTO.Username;
            c.Password = clientInputDTO.Password;
            c.CompanyName = clientInputDTO.CompanyName;
            await _mainAppContext.Clients.AddAsync(c);
            await _mainAppContext.SaveChangesAsync();
            apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = c
            };
            return Ok(apiResponse);
        }

        // Delete existing Client info
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            Client? c = await _mainAppContext.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (c != null)
            {
                _mainAppContext.Remove(c);
                await _mainAppContext.SaveChangesAsync();
                return Ok("Deleted");

            }

            return NotFound();
        }

        // Update existing Client info
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] ClientInputDTO clientInputDTO)
        {
            Client? c = await _mainAppContext.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (c != null)
            {
                c.Name = clientInputDTO.Name;
                c.Username = clientInputDTO.Username;
                c.Password = clientInputDTO.Password;
                c.CompanyName = clientInputDTO.CompanyName;

                await _mainAppContext.SaveChangesAsync();
                return Ok(c);

            }

            return NotFound();
        }


    }
}
