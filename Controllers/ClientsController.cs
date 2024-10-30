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

        //get all the clients info.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // entryPoint of DB comuniction
            var data = await _mainAppContext.Clients.ToListAsync();
            return Ok(data);
        }

        //get the info. of the client by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(int id)
        {

            Client? cl = await _mainAppContext.Clients.FirstOrDefaultAsync(c => c.Id == id);

            if (cl == null)
            {
                return NotFound("The resoucre is not found!");
            }

            return Ok(cl);

        }


        //get just clients info. or clients info. whith their projects
        [HttpGet("GetAllFromQuery")]
        public async Task<IActionResult> GetAll([FromQuery] string? Mode) {
            var ClientList = new List<ClienOutDTO>();
            if(Mode == null || Mode == "basic")
            {
                ClientList = await _mainAppContext.Clients
                 .Include(c => c.Projects)
                  .Select(c => new ClienOutDTO
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
                ClientList = await _mainAppContext.Clients
                .Include(c => c.Projects)
                 .Select(c => new ClienOutDTO
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

        //add  new client
        [HttpPost]
        public async Task<IActionResult>  CreateClient([FromBody] ClientInputDTO clientInputDTO)
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

        //add new client by admin
        [HttpPost("Register")]
        public async Task<IActionResult> CreateClientByAdmin([FromBody] ClientInputDTO clientInputDTO)
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

        //delete client 
        [HttpDelete("{id}")]
        public async Task<IActionResult>  DeleteClient(int id)
        {
            Client cl =await _mainAppContext.Clients.FirstOrDefaultAsync( c => c.Id == id);
            if (cl != null)
            {
                _mainAppContext.Remove(cl);
             await  _mainAppContext.SaveChangesAsync();
                return Ok("Deleted");

            }

            return NotFound();
        }


        //update the info. of the client 
        [HttpPut("{id}")]
        public async Task<IActionResult>  Update(int id, [FromBody] ClientInputDTO clientInputDTO)
        {
            Client cl =await _mainAppContext.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (cl != null)
            {
                cl.Name = clientInputDTO.Name;
                cl.Username = clientInputDTO.Username;
                cl.Password = clientInputDTO.Password;
                cl.CompanyName = clientInputDTO.CompanyName;

                await    _mainAppContext.SaveChangesAsync();
                return Ok(cl);

            }

            return NotFound();
        }
    }
}
