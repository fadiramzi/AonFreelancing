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
        public async Task<IActionResult> GetAll([FromQuery] string? Mode)
        {
            var clientOutputDTOs = new List<ClientOutputDTO>();
            if (Mode == null || Mode == "basic")
            {
                clientOutputDTOs = await _mainAppContext.Clients
                 .Include(c => c.Projects)
                  .Select(c => new ClientOutputDTO
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
                clientOutputDTOs = await _mainAppContext.Clients
                .Include(c => c.Projects)
                 .Select(c => new ClientOutputDTO
                 {
                     Id = c.Id,
                     CompanyName = c.CompanyName,
                     Name = c.Name,
                     Username = c.Username,
                     Projects = c.Projects.Select(p => new ProjectOutputDTO
                     {
                         Id = p.Id,
                         Title = p.Title,
                         Description = p.Description,

                     })
                 })
                .ToListAsync();
            }
            ApiResponse<object> apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = clientOutputDTOs
            };
            return Ok(apiResponse);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient([FromRoute] int id)
        {
            Client? client = await _mainAppContext.Clients.Include(c => c.Projects).FirstOrDefaultAsync(c => c.Id == id);
            if (client == null)
                return NotFound("The Resource is not found");

            ApiResponse<object> apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = new ClientOutputDTO(client)
            };

            return Ok(apiResponse);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClientInputDTO clientDTO)
        {
            Client client = new Client(clientDTO);
            await _mainAppContext.Clients.AddAsync(client);
            await _mainAppContext.SaveChangesAsync();

            ApiResponse<object> apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = new ClientOutputDTO(client)
            };

            return CreatedAtAction(nameof(GetClient), new { Id = client.Id }, apiResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ClientInputDTO clientInputDTO)
        {
            Client? client = await _mainAppContext.Clients.Include(c => c.Projects).FirstOrDefaultAsync(c => c.Id == id);
            if (client == null)
                return NotFound("The resource is not found");

            client.Username = clientInputDTO.Username ?? client.Username;
            client.Name = clientInputDTO.Name ?? client.Name;
            client.Password = clientInputDTO.Password ?? client.Password;
            client.CompanyName = clientInputDTO.CompanyName ?? client.CompanyName;

            await _mainAppContext.SaveChangesAsync();

            ApiResponse<object> apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = new ClientOutputDTO(client)
            };

            return Ok(apiResponse);

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Client? client = await _mainAppContext.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (client == null)
                return NotFound("The resource is not found");

            _mainAppContext.Clients.Remove(client);
            await _mainAppContext.SaveChangesAsync();

            ApiResponse<object> apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = "Deleted"
            };
            return Ok(apiResponse);
        }
    }
}
