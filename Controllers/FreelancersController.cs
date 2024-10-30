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
        // Read all freelancers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // entryPoint of DB comuniction
            var data = await _mainAppContext.Freelancers.ToListAsync();
            return Ok(data);
        }
        //api/freelancers/
        // Create freelancer 
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FreelancerInputDTO freelancerDTO)
        {
            // I created FreelancerInputDTO instead of Freelance to get rid of the (password required) bug
            // Api Response for validation
            ApiResponse<object> apiResponse;

            Freelancer f = new Freelancer();
            f.Name = freelancerDTO.Name; // Add new Name
            f.Username = freelancerDTO.Username; // Add new Username
            f.Password = freelancerDTO.Password; // Add new Password
            f.Skills = freelancerDTO.Skills; // Add new Skills

            await _mainAppContext.Freelancers.AddAsync(f); // Entity Async for add new item to the database 
            await _mainAppContext.SaveChangesAsync(); // saving the changes to the database
            apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = f
            };


            return Ok(apiResponse);
        }

        // api/freelancers/Register
        // Register freelancer
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] FreelancerDTO freelancerDTO)
        {
            ApiResponse<object> apiResponse;

            Freelancer f = new Freelancer();
            f.Name = freelancerDTO.Name; // Add new Name 
            f.Username = freelancerDTO.Username; // Add new Username
            f.Password = freelancerDTO.Password; // Add new Password
            f.Skills = freelancerDTO.Skills; // Add new Skills

            await _mainAppContext.Freelancers.AddAsync(f); // Entity Async for add new item to the database 
            await _mainAppContext.SaveChangesAsync(); // saving the changes to the database
            apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = f
            };


            return Ok(apiResponse);
        }

        // Read freelancer by id
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

        // Delete Freelancer by id 
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Freelancer? f = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
            if (f != null)
            {
                _mainAppContext.Remove(f); // Entity for deletion.
                await _mainAppContext.SaveChangesAsync(); // saving the changes to the database
                return Ok("Deleted");

            }

            return NotFound();
        }

        // update Freelancer by id (update name, username, password, skills)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FreelancerInputDTO freelancerDTO)
        {
            Freelancer? f = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
            if (f != null)
            {
                f.Name = freelancerDTO.Name; // update name
                f.Username = freelancerDTO.Username; // update username
                f.Password = freelancerDTO.Password; // update password
                f.Skills = freelancerDTO.Skills; // update skills

                await _mainAppContext.SaveChangesAsync(); // saving the changes to the database
                return Ok(f);

            }

            return NotFound();
        }

    }
}