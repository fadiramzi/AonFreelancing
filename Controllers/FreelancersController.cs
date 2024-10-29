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

        //Get all freelancers
        [HttpGet]
        public async Task <IActionResult> GetAllFreelancers()
        {
            // entryPoint of DB comuniction
            List<Freelancer> freelancers = await _mainAppContext.Freelancers.ToListAsync();
            return Ok(freelancers);
        }

        // Create a new Freelancer
        // "api/freelancers/Register"
        [HttpPost("Register")]
        public async Task<IActionResult> CreateFreelancer([FromBody] FreelancerInputDTO freelancerDTO)
        {
            ApiResponse<object> apiResponse;
           
            Freelancer freelancer = new Freelancer();
            freelancer.Name = freelancerDTO.Name;
            freelancer.Username = freelancerDTO.Username;
            freelancer.Password = freelancerDTO.Password;
            freelancer.Skills = freelancerDTO.Skills;
           
            await _mainAppContext.Freelancers.AddAsync(freelancer);
            await _mainAppContext.SaveChangesAsync();
            apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = freelancer
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
