using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.DTOs.ClientDTO;
using AonFreelancing.Models.DTOs.ProjectDTOs;
using AonFreelancing.Models.DTOs.ResponseDTOs;
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
            ApiResponseDTO<object> apiResponseDTO;
            if (loadProjects == null || loadProjects == 0)
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
            else if (loadProjects == 1)
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
            {
                apiResponseDTO = new ApiResponseDTO<object>
                {
                    IsSuccess = false,
                    Error = new Error { Code = 400, Message = $"{loadProjects} is not a valid value." }
                };
                return BadRequest(apiResponseDTO);
            }
            apiResponseDTO = new ApiResponseDTO<object>
            {
                IsSuccess = true,
                Results = ClientList,
            };
            return Ok(apiResponseDTO);
        }
        //Get client by Id
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetClientById(int Id, [FromQuery] int? loadProjects)
        {
            var client = new ClientDTO();
            ApiResponseDTO<object> apiResponseDTO;
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
            {
                apiResponseDTO = new()
                {
                    IsSuccess = false,
                    Error = new Error() { Code = 400, Message = $"{loadProjects} is not a valid value." }
                };
                return BadRequest(apiResponseDTO);
            }

            if (client == null)
            {
                apiResponseDTO = new()
                {
                    IsSuccess = false,
                    Error = new Error() { Code = 404, Message = $"No client with {Id} ID."}
                };
                return NotFound(apiResponseDTO);
            }
            apiResponseDTO = new()
            {
                IsSuccess = true,
                Results = client
            };
            return Ok(apiResponseDTO);

        }

        //Creating a new client
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] ClientInputDTO clientDTO)
        {
            ApiResponseDTO<object> apiResponseDTO;
            Client client = new();
            client.User = new User();
            client.CompanyName = clientDTO.CompanyName;
            client.User.Name = clientDTO.Name;
            client.User.Username = clientDTO.Username;
            client.User.Password = clientDTO.Password;
            await _mainAppContext.Clients.AddAsync(client);
            await _mainAppContext.SaveChangesAsync();

            apiResponseDTO = new()
            {
                IsSuccess = true,
                Results = new ClientDTO()
                {
                    Username = client.User.Username,
                    Name = client.User.Name,
                    Id = client.Id,
                    CompanyName = client.CompanyName,
                }
            };

            return Ok(apiResponseDTO);
        }

        //Removing a client
        [HttpDelete]
        public async Task<IActionResult> RemoveClientById([FromQuery] int id)
        {
            ApiResponseDTO<object> apiResponseDTO;
            Client? client = await _mainAppContext.Clients.Include(c=>c.User).FirstOrDefaultAsync(c=>c.Id == id);
            if (client is null)
            {
                apiResponseDTO = new()
                {
                    IsSuccess = false,
                    Error = new Error()
                    {
                        Code = 404, Message = $"Client {id} is Not Found.",
                    }
                };

                return NotFound(apiResponseDTO);
            }
            _mainAppContext.Clients.Remove(client);
            await _mainAppContext.SaveChangesAsync();


            apiResponseDTO = new()
            {
                IsSuccess = true,
                Results = new ClientDTO()
                {
                    Username = client.User.Username,
                    Name = client.User.Name,
                    Id = client.Id,
                    CompanyName = client.CompanyName,
                },
            };
            return Ok(apiResponseDTO);
        }

        //Updating Client by Id
        [HttpPut]
        public async Task<IActionResult> UpdateClientById([FromQuery] int id, [FromBody] ClientInputDTO clientDTO)
        {
            ApiResponseDTO<object> apiResponseDTO;

            Client? client = await _mainAppContext.Clients.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            if (client is null)
            {
                apiResponseDTO = new()
                {
                    IsSuccess = false,
                    Error = new Error()
                    {
                        Code = 404,
                        Message = $"Client {id} is Not Found.",
                    }
                };

                return NotFound(apiResponseDTO);
            }

            // Updating client values
            client.User.Name = clientDTO.Name;
            client.User.Username = clientDTO.Username;
            client.CompanyName = clientDTO.CompanyName;
            client.User.Password = clientDTO.Password;

            await _mainAppContext.SaveChangesAsync();
            apiResponseDTO = new()
            {
                IsSuccess = true,
                Results = new ClientDTO()
                {
                    Username = client.User.Username,
                    Name = client.User.Name,
                    Id = client.Id,
                    CompanyName = client.CompanyName,
                },
            };
            return Ok(apiResponseDTO);
        }

    }
}
