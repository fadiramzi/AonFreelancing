using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace AonFreelancing.Controllers
{
    [Route("api/freelancers")]
    [ApiController]
    public class FreelancersController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;

        public FreelancersController(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }
        [HttpGet]
        public async Task<IActionResult > GetAll()
        {
           var freelancerDto=new List<FreelancerOutDTO>();

            freelancerDto = await _mainAppContext.Freelancers
             .Include(c => c.Projects).Select(f => new FreelancerOutDTO
             {
                 Id = f.Id,
                 Name = f.Name, 
                 Username = f.Username,
                 Skills = f.Skills,
             })
                 .ToListAsync();
            return Ok(freelancerDto);
        }
        //api/freelancers/
        [HttpPost]
        public async Task< IActionResult>Create([FromBody] FreelancerDTO freelancer) {

            Freelancer? fr = new Freelancer()
            {
                Name = freelancer.Name,
                Username = freelancer.Username,
                Skills = freelancer.Skills,
                Password = freelancer.Password


            };
            _mainAppContext.Freelancers.Add(fr);
            _mainAppContext.SaveChanges();

            return Ok(fr);
        }

        //api/freelancers/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] FreelancerDTO freelancerDTO)
        {
            ApiResponse<object> apiResponse;
           
            Freelancer f = new Freelancer();
            f.Name = freelancerDTO.Name;
            f.Username = freelancerDTO.Username;
            f.Password = freelancerDTO.Password;
            f.Skills = freelancerDTO.Skills;
           
            await _mainAppContext.Freelancers.AddAsync(f);
            await _mainAppContext.SaveChangesAsync();
            apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = f
            };
           

            return Ok(apiResponse);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreelancer([Required] int? LoadProject)
        {
            var ClientList = new List<ClientDTO>();
            if (LoadProject==1)
            {
                ClientList = await _mainAppContext.Clients
                 .Include(c => c.Projects)
                  .Select(c => new ClientDTO
                  {
                      Id = c.Id,
                      CompanyName = c.CompanyName,
                      Name = c.Name,
                      Username = c.Username,
                      Projects=c.Projects.Select(c=>new ProjectOutDTO
                      {
                          Id = c.Id,
                          Title =c.Title,
                          Description=c.Description,    
                          ClientId=c.ClientId,

                      })
                      
                  })
                 .ToListAsync();

                return Ok(ClientList);

            }
           
                return BadRequest();

              
        }

        [HttpDelete("{id}")]
        public async Task< IActionResult>Delete(int id)
        {
            Freelancer ? f =await _mainAppContext.Freelancers.FirstOrDefaultAsync(f=>f.Id == id);
            if(f!= null)
            {
               _mainAppContext.Remove(f);
              await _mainAppContext.SaveChangesAsync();
                return Ok("Deleted"); 

            }

            return NotFound();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id,[FromBody] FreelancerDTO freelancer)
        {
            Freelancer? f = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
            if (f != null)
            {
                f.Name = freelancer.Name;
                f.Skills = freelancer.Skills;
                f.Username = freelancer.Username;
                f.Password= freelancer.Password;
             
                 await _mainAppContext.SaveChangesAsync();
                return Ok(f);

            }

            return NotFound();
        }

    
    }
}
