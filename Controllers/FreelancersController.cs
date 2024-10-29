using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            // entryPoint of DB comuniction
            List<FreelancerOutputDTO> FreelancerOutputDTOs = await _mainAppContext.Freelancers.Select(freelancer => new FreelancerOutputDTO(freelancer)).ToListAsync();
            ApiResponse<object> apiResponse = new ApiResponse<object>() { IsSuccess = true, Results = FreelancerOutputDTOs };

            return Ok(apiResponse);
        }

        //api/freelancers/
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FreelancerInputDTO freelancerInputDTO)
        {
            Freelancer freelancer = new Freelancer(freelancerInputDTO);

            await _mainAppContext.Freelancers.AddAsync(freelancer);
            await _mainAppContext.SaveChangesAsync();

            ApiResponse<object> apiResponse = new ApiResponse<object>()
            {
                IsSuccess = true,
                Results = new FreelancerOutputDTO(freelancer)
            };

            return CreatedAtAction(nameof(GetFreelancer), new { Id = freelancer.Id }, apiResponse);
        }

        //api/freelancers/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] FreelancerInputDTO freelancerDTO)
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
                Results = new FreelancerOutputDTO(f)
            };


            return Ok(apiResponse);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreelancer(int id)
        {

            Freelancer? fr = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);

            if (fr == null)
                return NotFound("The resource is not found!");

            ApiResponse<object> apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = new FreelancerOutputDTO(fr)
            };
            return Ok(apiResponse);

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Freelancer? f = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
            if (f != null)
            {
                _mainAppContext.Remove(f);
                await _mainAppContext.SaveChangesAsync();
                ApiResponse<object> apiResponse = new ApiResponse<object>
                {
                    IsSuccess = true,
                    Results = "Deleted"
                };
                return Ok(apiResponse);
            }

            return NotFound();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FreelancerInputDTO freelancerInputDTO)
        {
            Freelancer? freelancer = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
            if (freelancer != null)
            {
                freelancer.Username = freelancerInputDTO?.Username ?? freelancer.Username;
                freelancer.Name = freelancerInputDTO.Name ?? freelancer.Name;
                freelancer.Skills = freelancerInputDTO.Skills ?? freelancer.Skills;
                freelancer.Password = freelancerInputDTO.Password ?? freelancer.Password;

                await _mainAppContext.SaveChangesAsync();
                ApiResponse<object> apiResponse = new ApiResponse<object>
                {
                    IsSuccess = true,
                    Results = new FreelancerOutputDTO(freelancer)
                };
                return Ok(apiResponse);
            }
            return NotFound();
        }
    }
}
