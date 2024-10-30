using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        //get all info. of freelancers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // entryPoint of DB comuniction
            var data = await _mainAppContext.Freelancers.ToListAsync();
            return Ok(data);
        }

        //get the info. of the freelancer by id
        [HttpGet("FreelanceId")]
        public async Task<IActionResult> GetFreelancer(int id)
        {

            Freelancer? fr = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);

            if (fr == null)
            {
                return NotFound("The resoucre is not found!");
            }

            return Ok(fr);

        }

        //get just freelance info.or freelances info. whith their projects
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreelancer([FromQuery] string? loadProjects,int id)
        {
            FreelancerOutDTO? freelancerOutDTO = new FreelancerOutDTO();
            if (loadProjects == null || loadProjects == "0")
            {
              freelancerOutDTO = await _mainAppContext.Freelancers
                 .Include(f => f.Projects)
                 .Select(f => new FreelancerOutDTO
                  {
                      Id = f.Id,
                      Skills = f.Skills,
                      Name = f.Name,
                      Username = f.Username
                  })
                 .FirstOrDefaultAsync(f => f.Id == id);

            }
            if (loadProjects == "1")
            {
                freelancerOutDTO = await _mainAppContext.Freelancers
                .Include(f => f.Projects)
                 .Select(f => new FreelancerOutDTO
                 {
                     Id = f.Id,
                     Skills = f.Skills,
                     Name = f.Name,
                     Username = f.Username,
                    Projects = f.Projects.Select(p => new Project
                     {
                         Id = p.Id,
                         Title = p.Title,
                         Description = p.Description,

                     })
                 })
                 .FirstOrDefaultAsync(f => f.Id == id);
            }

            return Ok(freelancerOutDTO);
        }

        //api/freelancers/
        [HttpPost]
        public async Task<IActionResult> CreateFreelance([FromBody] FreelancerInputDTO freelancerInputDTO)
        {
            
            Freelancer f = new Freelancer();
            f.Name = freelancerInputDTO.Name;
            f.Username = freelancerInputDTO.Username;
            f.Password = freelancerInputDTO.Password;
            f.Skills = freelancerInputDTO.Skills;

            await _mainAppContext.Freelancers.AddAsync(f);
            await _mainAppContext.SaveChangesAsync();
          


            return Ok(f);
        }

        //api/freelancers/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] FreelancerInputDTO freelancerInputDTO)
        {
            ApiResponse<object> apiResponse;
           
            Freelancer f = new Freelancer();
            f.Name = freelancerInputDTO.Name;
            f.Username = freelancerInputDTO.Username;
            f.Password = freelancerInputDTO.Password;
            f.Skills = freelancerInputDTO.Skills;
           
            await _mainAppContext.Freelancers.AddAsync(f);
            await _mainAppContext.SaveChangesAsync();
            apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = f
            };
           

            return Ok(apiResponse);
        }

        
        //delete freelance
        [HttpDelete("{id}")]
        public async Task<IActionResult>  Delete(int id)
        {
            Freelancer f = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f=>f.Id == id);
            if(f!= null)
            {
                _mainAppContext.Remove(f);
               await _mainAppContext.SaveChangesAsync();
                return Ok("Deleted");

            }

            return NotFound();
        }


        //update info. of freelance
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FreelancerInputDTO freelancerInputDTO)
        {
            Freelancer f = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
            if (f != null)
            {
                f.Name = freelancerInputDTO.Name;
                f.Username = freelancerInputDTO.Username;
                f.Password = freelancerInputDTO.Password;
                f.Skills = freelancerInputDTO.Skills;

                await _mainAppContext.SaveChangesAsync();
                return Ok(f);

            }

            return NotFound();
        }


    }
}
