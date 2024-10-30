using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

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

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? Mode) {
            var ClientList = new List<ClientDTO>();
            if(Mode == null || Mode == "basic")
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
            if(Mode == "r")
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
        //api/clients/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] ClientInputDTO clientDTO)
        {
            ApiResponse<object> apiResponse;

            Client f = new Client();
            f.Name = clientDTO.Name;
            f.Username = clientDTO.Username;
            f.Password = clientDTO.Password;
            f.CompanyName = clientDTO.CompanyName;
          

            await _mainAppContext.Clients.AddAsync(f);
            await _mainAppContext.SaveChangesAsync();
            apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = f
            };


            return Ok(apiResponse);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreelancer(int id)
        {

            Client? fr = await _mainAppContext.Clients.FirstOrDefaultAsync(f => f.Id == id);

            if (fr == null)
            {
                return NotFound("The resoucre is not found!");
            }

            return Ok(fr);

        }


        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Client f = _mainAppContext.Clients.FirstOrDefault(f => f.Id == id);
            if (f != null)
            {
                _mainAppContext.Remove(f);
                _mainAppContext.SaveChanges();
                return Ok("Deleted");

            }

            return NotFound();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] ClientInputDTO client)
        {
            Client f = _mainAppContext.Clients.FirstOrDefault(f => f.Id == id);
            if (f != null)
            {
                f.Name = client.Name;
                f.Username= client.Username;
                f.CompanyName= client.CompanyName;
                _mainAppContext.SaveChanges();
                return Ok(f);

            }

            return NotFound();
        }
    }
}
