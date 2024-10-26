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
        public IActionResult GetAll()
        {
            // entryPoint of DB comuniction
            var data = _mainAppContext.Freelancers.ToList();
            return Ok(data);
        }
        //api/freelancers/
        [HttpPost]
        public IActionResult Create([FromBody] Freelancer freelancer) {
            _mainAppContext.Freelancers.Add(freelancer);
            _mainAppContext.SaveChanges(); 

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
        public async Task<IActionResult> GetFreelancer(int id)
        {
           
            Freelancer? fr = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
           
            if (fr == null)
            {
                return NotFound("The resoucre is not found!");
            }

            return Ok(fr);

        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Freelancer f = _mainAppContext.Freelancers.FirstOrDefault(f=>f.Id == id);
            if(f!= null)
            {
                _mainAppContext.Remove(f);
                _mainAppContext.SaveChanges();
                return Ok("Deleted");

            }

            return NotFound();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Freelancer freelancer)
        {
            Freelancer f = _mainAppContext.Freelancers.FirstOrDefault(f => f.Id == id);
            if (f != null)
            {
                f.Name = freelancer.Name;

                _mainAppContext.SaveChanges();
                return Ok(f);

            }

            return NotFound();
        }



    }
}
