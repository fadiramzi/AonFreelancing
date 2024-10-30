using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Xml;

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
        public async Task<IActionResult> GetAllAsync()
        {
            // entryPoint of DB comuniction
            List<FreelancerOutputDTO> FreelancerOutputDTOs = await _mainAppContext.Freelancers.Select(freelancer => new FreelancerOutputDTO(freelancer)).ToListAsync();
            ApiResponse<object> apiResponse = new ApiResponse<object>() { IsSuccess = true, Results = FreelancerOutputDTOs };

            return Ok(apiResponse);
        }

        //api/freelancers/
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] FreelancerInputDTO freelancerInputDTO)
        {
            Freelancer freelancer = new Freelancer(freelancerInputDTO);

            await _mainAppContext.Freelancers.AddAsync(freelancer);
            await _mainAppContext.SaveChangesAsync();

            ApiResponse<object> apiResponse = new ApiResponse<object>()
            {
                IsSuccess = true,
                Results = new FreelancerOutputDTO(freelancer)
            };

            return CreatedAtAction(nameof(GetFreelancerAsync), new { id = freelancer.Id, loadProjects = 0 }, apiResponse);
        }

        //api/freelancers/Register
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody] FreelancerInputDTO freelancerDTO)
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
        public async Task<IActionResult> GetFreelancerAsync([FromRoute] int id,
            [FromQuery][BindRequired][Range(0, 1, ErrorMessage = "loadProjects must be either 0 or 1")] int loadProjects)
        {
            Freelancer? freelancer;
            if (loadProjects == 0)
                freelancer = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
            else
                freelancer = await _mainAppContext.Freelancers.Include(f => f.Projects).FirstOrDefaultAsync(f => f.Id == id);

            if (freelancer == null)
                return NotFound(new ApiResponse<object>() {Error = new Error { Message = "The resource is not found!", Code = "404" } });

            ApiResponse<object> apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = new FreelancerOutputDTO(freelancer) { Projects = freelancer.Projects.Select(p => new ProjectOutputDTO(p)) }
            };
            return Ok(apiResponse);

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            Freelancer? freelancer = await _mainAppContext.Freelancers.Include(f => f.Projects).FirstOrDefaultAsync(f => f.Id == id);
            if (freelancer != null)
            {
                freelancer.Projects = [];//breaks relationship between this freelancer and its assigned projects
                _mainAppContext.Remove(freelancer);
                await _mainAppContext.SaveChangesAsync();
                ApiResponse<object> apiResponse = new ApiResponse<object>
                {
                    IsSuccess = true,
                    Results = "Deleted"
                };
                return Ok(apiResponse);
            }

            return NotFound(new ApiResponse<object> { Error = new Error { Message = "Resource not found", Code = "404" } });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] FreelancerInputDTO freelancerInputDTO)
        {
            Freelancer? freelancer = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
            if (freelancer != null)
            {
                freelancer.Username = freelancerInputDTO.Username;
                freelancer.Name = freelancerInputDTO.Name;
                freelancer.Skills = freelancerInputDTO.Skills;
                freelancer.Password = freelancerInputDTO.Password;

                await _mainAppContext.SaveChangesAsync();
                ApiResponse<object> apiResponse = new ApiResponse<object>
                {
                    IsSuccess = true,
                    Results = new FreelancerOutputDTO(freelancer)
                };
                return Ok(apiResponse);
            }
            return NotFound(new ApiResponse<object> { Error = new Error { Message = "Resource not found", Code = "404" } });
        }
    }
}
