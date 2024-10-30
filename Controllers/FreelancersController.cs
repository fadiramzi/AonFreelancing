using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace AonFreelancing.Controllers
{
    // Implement endpoint api/v1/freelancers/{id}?loadProjects=x, where x = 1 or 0
      [Route("api/v1/freelancers")]
    [ApiController]
    public class FreelancersController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;

        public FreelancersController(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        // get all freelancers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // entryPoint of DB comuniction
            var data = await _mainAppContext.Freelancers.ToListAsync();
            return Ok(data);
        }
        //retrieve a single Freelancer by his id
        [HttpGet("BasicGetById")]
        public async Task<IActionResult> GetFreelancerById(int id)
        {
            FreelancerBasicDTO freelancerBasicDTO = new FreelancerBasicDTO();
            var freelancer = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
            if (freelancer == null)
            {
                return NotFound();
            }
            freelancerBasicDTO.Id = freelancer.Id;
            freelancerBasicDTO.Name = freelancer.Name;
            freelancerBasicDTO.Username = freelancer.Username;
            freelancerBasicDTO.Skills = freelancer.Skills;
            
            return Ok(freelancerBasicDTO);
        }

        // Retrieve freelancer info only, or freelancer info with his projcts based on Query Parameter 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreelancerAndProjects(int id, [FromQuery] int? loadProjects)
        {
            FreelancerOutDTO? freelnacer = new FreelancerOutDTO();
            if (loadProjects == null || loadProjects == 0)
            {
                freelnacer = await _mainAppContext.Freelancers
                     .Include(f => f.Projects)
                   .Select(f => new FreelancerOutDTO
                   {
                       Id = f.Id,
                       Name = f.Name,
                       Username = f.Username,
                       Skills = f.Skills,
                   })
                  .FirstOrDefaultAsync(f => f.Id == id);
            }
            if (loadProjects == 1)
            {
                freelnacer = await _mainAppContext.Freelancers
                .Include(f => f.Projects)
                 .Select(f => new FreelancerOutDTO
                 {
                     Id = f.Id,
                     Name = f.Name,
                     Username = f.Username,
                     Skills = f.Skills,
                     Projects = f.Projects.Select(p => new ProjectOutDTO
                     {
                         Id = p.Id,
                         Title = p.Title,
                         Description = p.Description,

                     })
                 })
               .FirstOrDefaultAsync(f => f.Id == id);
            }
            return Ok(freelnacer);
        }

        //HttpPost and HttpPost("Register") to create new freelnacer by user or system admin
        //api/freelancers/
        [HttpPost]
        public async Task<IActionResult> CreateFreelancer([FromBody] FreelancerInDTO freelancerInDTO)
        {
            Freelancer? f = new Freelancer();
            f.Name = freelancerInDTO.Name;
            f.Username = freelancerInDTO.Username;
            f.Password = freelancerInDTO.Password;
            f.Skills = freelancerInDTO.Skills;
            await _mainAppContext.Freelancers.AddAsync(f);
            await _mainAppContext.SaveChangesAsync();
            return Ok(f);
        }
        //api/freelancers/Register
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterFreelancer([FromBody] FreelancerInDTO freelancerInDTO)
        {
            ApiResponse<object> apiResponse;
           
            Freelancer? f = new Freelancer();
            f.Name = freelancerInDTO.Name;
            f.Username = freelancerInDTO.Username;
            f.Password = freelancerInDTO.Password;
            f.Skills = freelancerInDTO.Skills;
           
            await _mainAppContext.Freelancers.AddAsync(f);
            await _mainAppContext.SaveChangesAsync();
            apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = f
            };
           

            return Ok(apiResponse);
        }

        // delete existing freelancer
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFreelancer(int id)
        {
            Freelancer? f = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
            if (f!= null)
            {
                _mainAppContext.Remove(f);
                await _mainAppContext.SaveChangesAsync();
                return Ok("Deleted");

            }

            return NotFound();
        }

        // update existing freelancer
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFreelancer(int id, [FromBody] FreelancerInDTO freelancerInDTO)
        {
            Freelancer? f = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
            if (f != null)
            {
                f.Name = freelancerInDTO.Name;

               await _mainAppContext.SaveChangesAsync();
                return Ok(f);

            }

            return NotFound();
        }



    }
}
