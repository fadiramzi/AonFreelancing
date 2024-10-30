using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
        public async Task<IActionResult> GetAllAsync([FromQuery] string? Mode)
        {
            var clientOutputDTOs = new List<ClientOutputDTO>();
            if (Mode == null || Mode == "basic")
            {
                clientOutputDTOs = await _mainAppContext.Clients
                  //.Include(c => c.Projects)
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
                 .ThenInclude(p => p.Freelancers)
                 .Select(c => new ClientOutputDTO
                 {
                     Id = c.Id,
                     CompanyName = c.CompanyName,
                     Name = c.Name,
                     Username = c.Username,
                     Projects = c.Projects.Select(p => new ProjectOutputDTO(p)
                     {
                         Freelancers = p.Freelancers.Select(f => new FreelancerOutputDTO(f))
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
        public async Task<IActionResult> GetClientAsync([FromRoute] int id,
            [FromQuery][BindRequired][Range(0, 1, ErrorMessage = "loadProjects must be either 0 or 1")] int loadProjects)
        {
            Client? client;
            if (loadProjects == 0)
                client = await _mainAppContext.Clients.FirstOrDefaultAsync(c => c.Id == id);
            else
                client = await _mainAppContext.Clients.Include(c => c.Projects).FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                return NotFound(new ApiResponse<object> { Error = new Error { Message = "Resource not found", Code = "404" } });

            ApiResponse<object> apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = new ClientOutputDTO(client) { Projects = client.Projects.Select(p => new ProjectOutputDTO(p)) }
            };
            return Ok(apiResponse);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ClientInputDTO clientDTO)
        {
            Client client = new Client(clientDTO);
            await _mainAppContext.Clients.AddAsync(client);
            await _mainAppContext.SaveChangesAsync();

            ApiResponse<object> apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = new ClientOutputDTO(client)
            };

            return CreatedAtAction(nameof(GetClientAsync), new { id = client.Id , loadProjects = 0}, apiResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] ClientInputDTO clientInputDTO)
        {
            Client? client = await _mainAppContext.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (client == null)
                return NotFound(new ApiResponse<object> { Error = new Error { Message = "Resource not found", Code = "404" } });

            client.Username = clientInputDTO.Username;
            client.Name = clientInputDTO.Name;
            client.Password = clientInputDTO.Password;
            client.CompanyName = clientInputDTO.CompanyName;

            await _mainAppContext.SaveChangesAsync();

            ApiResponse<object> apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = new ClientOutputDTO(client)
            };

            return Ok(apiResponse);

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            Client? client = await _mainAppContext.Clients.Include(c=>c.Projects).FirstOrDefaultAsync(c => c.Id == id);
            if (client == null)
                return NotFound(new ApiResponse<object> { Error = new Error { Message = "Resource not found", Code = "404" } });

            client.Projects = [];//delete associated projects
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
