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
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _mainAppContext.Freelancers.ToListAsync();
            return Ok(data);
        }
        //api/freelancers/
        [HttpPost]
        public async Task<IActionResult>  Create([FromBody] Freelancer freelancer) 
        {
            _mainAppContext.Freelancers.Add(freelancer);
            await  _mainAppContext.SaveChangesAsync(); 

            return CreatedAtAction("Create", new { Id = freelancer.Id }, freelancer);
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
        public async Task<IActionResult> GetFreelancer(int id , [FromQuery] int? loadprojects )
        {
            if(loadprojects == null) 
            {
                return BadRequest("not found");

            }
            if (loadprojects == 0 || loadprojects == 1)
            {
                Freelancer? fr = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);

                if (fr == null)
                {
                    return NotFound("The resoucre is not found!");
                }

                return Ok(fr);
            }

            return BadRequest();

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DelateID(int id)
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

        [HttpPut("{id}")]
        public async Task<IActionResult>  Update(int id, [FromBody] Freelancer freelancer)
        {
            Freelancer f = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
            if (f != null)
            {
                f.Name = freelancer.Name;

                await _mainAppContext.SaveChangesAsync();
                return Ok(f);

            }

            return NotFound();
        }



    }
}
