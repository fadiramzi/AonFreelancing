using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
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

        [HttpGet("{id}")]
        public async Task<ActionResult> GetClientByIdAsync(int id)
        {
            var response = new ApiResponse<ClientDTO?>();
            
            try
            {
                var clientList = await _mainAppContext.Clients
                    .Include(c => c.Projects)
                    .Select(c => new ClientDTO
                    {
                        Id = c.Id,
                        CompanyName = c.CompanyName,
                        Name = c.Name,
                        Username = c.Username,
                        Projects = c.Projects.Select(p => new ProjectOutDTO()
                        {
                            Id = p.Id,
                            ClientId = p.ClientId,
                            Description = p.Description,
                            CreatedAt = p.CreatedAt,
                            Title = p.Title
                        })
                    }).FirstOrDefaultAsync(c => c.Id == id);

                if (clientList == null)
                {
                    response.IsSuccess = false;
                    response.Error = new Error()
                    {
                        Code = "404",
                        Message = "Client does not exist",
                    };
                }
                response.IsSuccess = true;
                response.Results = clientList;
                
                return Ok(response);
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "500",
                    Message = e.Message,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] string? mode) {
            var clientList = new List<ClientDTO>();
            if (mode == null || mode == "basic")
            {
                clientList = await _mainAppContext.Clients
                  .Select(c => new ClientDTO
                  {
                      Id = c.Id,
                      CompanyName = c.CompanyName,
                      Name = c.Name,
                      Username = c.Username,
                      
                  })
                 .ToListAsync();
            }
            if (mode == "r")
            {
                clientList = await _mainAppContext.Clients
                    .Include(c => c.Projects)
                    .Select(c => new ClientDTO
                    {
                        Id = c.Id,
                        CompanyName = c.CompanyName,
                        Name = c.Name,
                        Username = c.Username,
                        Projects = c.Projects.Select(p => new ProjectOutDTO()
                        {
                            Id = p.Id,
                            Title = p.Title,
                            Description = p.Description,
                            ClientId = p.ClientId,

                        })
                    })
                    .ToListAsync();
            }
           
            return Ok(clientList);
        }

        [HttpPost("RegisterClient")]
        public async Task<IActionResult> CreateClientAsync([FromBody] ClientInputDTO clientDto)
        {
            var response = new ApiResponse<ClientDTO>();
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "422",
                    Message = "Invalid client data",
                };
                return UnprocessableEntity(response);
            }

            try
            {
                var client = new Client()
                {
                    CompanyName = clientDto.CompanyName,
                    Username = clientDto.Username,
                    Password = clientDto.Password,
                    Name = clientDto.Name,
                };
            
                _mainAppContext.Clients.Add(client);
                await _mainAppContext.SaveChangesAsync();

                var clientOutDto = new ClientDTO()
                {
                    Id = client.Id,
                    Name = clientDto.Name,
                    Username = clientDto.Username,
                    CompanyName = clientDto.CompanyName,
                    Projects = new List<ProjectOutDTO>()
                };
            
                response.IsSuccess = true;
                response.Results = clientOutDto;

                return CreatedAtAction(nameof(GetClientByIdAsync), new {id = clientOutDto.Id},response);
                
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "500",
                    Message = e.Message,
                };
                return StatusCode(500 ,response);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClientAsync(int id,[FromBody] ClientInputDTO clientDto)
        {
            var response = new ApiResponse<ClientDTO>();

            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.Error = new Error
                {
                    Code = "400",
                    Message = "Invalid input data"
                };
                return BadRequest(response);
            }
            
            try
            {
                var client = await _mainAppContext.Clients
                    .Include(c => c.Projects)
                    .FirstOrDefaultAsync(c => c.Id == id);
                if (client == null)
                {
                    response.IsSuccess = false;
                    response.Error = new Error()
                    {
                        Code = "404",
                        Message = "Client not found",
                    };
                }

                client!.Name = clientDto.Name;
                client.Username = clientDto.Username;
                client.CompanyName = clientDto.CompanyName;
                client.Password = clientDto.Password;

                await _mainAppContext.SaveChangesAsync();

                if (client.Projects != null)
                {
                    var updatedClientDto = new ClientDTO
                    {
                        Id = client.Id,
                        Name = client.Name,
                        Username = client.Username,
                        CompanyName = client.CompanyName,
                        Projects = client.Projects.Select(p => new ProjectOutDTO()
                        {
                            Id = p.Id,
                            Title = p.Title,
                            Description = p.Description,
                            ClientId = p.ClientId,
                            FreelancerId = p.FreelancerId,
                            CreatedAt = p.CreatedAt,
                        })
                    };

                    response.IsSuccess = true;
                    response.Results = updatedClientDto;
                }

                return Ok(response);
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "500",
                    Message = e.Message,
                };
                return StatusCode(500 ,response);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClientAsync(int id)
        {
            var response = new ApiResponse<string>();

            try
            {
                var client = await _mainAppContext.Clients
                    .Include(c=>c.Projects)
                    .FirstOrDefaultAsync(c => c.Id == id);
                if (client == null)
                {
                    response.IsSuccess = false;
                    response.Error = new Error()
                    {
                        Code = "404",
                        Message = "Client not found",
                    };
                    return NotFound(response);
                }
                client.Projects = null;
                _mainAppContext.Clients.Remove(client);
                await _mainAppContext.SaveChangesAsync();
                
                response.IsSuccess = true;
                response.Results = "Client was successfully deleted.";
                return Ok(response);
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "500",
                    Message = e.Message,
                };
                return StatusCode(500 ,response);
            }
        }
    }
}
