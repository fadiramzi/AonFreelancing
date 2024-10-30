using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs.FreelancerDTOs;
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
        public async Task<IActionResult> GetAllFreelancers()
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

        // Get Freelancer by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreelancerById(int id)
        {
            Freelancer? freeelancer = await _mainAppContext.Freelancers.FindAsync(id);
            if (freeelancer == null)
                return NotFound($"Freelancer {id} is not found.");

            FreelancerOutDTO freelancerDTO = new()
            {
                Id = freeelancer.Id,
                Name = freeelancer.Name,
                Username = freeelancer.Username,
                Skills = freeelancer.Skills,
            };

            return Ok(freelancerDTO);
        }

        // Remove Freelancer by id
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFreelancerById(int id)
        {
            Freelancer? freelancer = await _mainAppContext.Freelancers.FindAsync(id);
            if(freelancer == null)
                return NotFound($"Freelancer { id } is not found.");
            _mainAppContext.Remove(freelancer);
            await _mainAppContext.SaveChangesAsync();
            return NoContent();
        }

        // Updating Freelancer by Id
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFreelancerById(int id, [FromBody] FreelancerInputDTO freelancerDTO)
        {
            Freelancer? freelancer = await _mainAppContext.Freelancers.FindAsync(id);
            if (freelancer == null)
                return NotFound($"Freelancer { id } is not found.");
            freelancer.Name = freelancerDTO.Name; 
            freelancer.Username = freelancerDTO.Username;
            freelancer.Password = freelancerDTO.Password;
            freelancer.Skills = freelancerDTO.Skills;

            await _mainAppContext.SaveChangesAsync();
            return Ok();
        }


    }
}
