using System.ComponentModel.DataAnnotations;
using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs.FreelancerDTOs;
using AonFreelancing.Models.DTOs.ProjectDTOs;
using AonFreelancing.Models.DTOs.ResponseDTOs;
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
            List<FreelancerOutDTO> freelancers = await _mainAppContext.Freelancers
                .Include(f => f.User)
                .Select(f => new FreelancerOutDTO()
                {
                    Id = f.Id,
                    Name = f.User.Name,
                    Username = f.User.Username,
                    Skills = f.Skills,
                })
                .ToListAsync();
            
            ApiResponseDTO<List<FreelancerOutDTO>> apiResponse= new()
            {
                IsSuccess = true,
                Results = freelancers
            };

            return Ok(apiResponse);
        }

        // Create a new Freelancer
        // "api/freelancers/Register"
        [HttpPost("Register")]
        public async Task<IActionResult> CreateFreelancer([FromBody] FreelancerInputDTO freelancerDTO)
        {
            Freelancer freelancer = new();
            freelancer.User = new User();
            freelancer.User.Name = freelancerDTO.Name;
            freelancer.User.Username = freelancerDTO.Username;
            freelancer.User.Password = freelancerDTO.Password;
            freelancer.Skills = freelancerDTO.Skills;

            await _mainAppContext.Freelancers.AddAsync(freelancer);
            await _mainAppContext.SaveChangesAsync();
            
            ApiResponseDTO<object> apiResponse= new()
            {
                IsSuccess = true,
                Results = freelancer
            };

            return Ok(apiResponse);
        }

        // Get Freelancer by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreelancerById(int id, [FromQuery][Required] int loadProjects)
        {
            FreelancerOutDTO freelancerDTO;
            ApiResponseDTO<object> apiResponse;
            if (loadProjects == 0)
            {
                Freelancer? freeelancer = await _mainAppContext.Freelancers
                    .Include(f => f.User)
                    .FirstOrDefaultAsync(f => f.Id == id);
                if (freeelancer == null)
                {
                    apiResponse = new()
                    {
                        IsSuccess = false,
                        Error = new Error() { Code = 404, Message = $"Freelancer {id} Not Found." },
                    };
                    return NotFound(apiResponse);
                }

                freelancerDTO = new()
                {
                    Id = freeelancer.Id,
                    Name = freeelancer.User.Name,
                    Username = freeelancer.User.Username,
                    Skills = freeelancer.Skills,
                };
            }
            else if(loadProjects == 1)
            {
                Freelancer? freeelancer = await _mainAppContext.Freelancers
                    .Include(f => f.User)
                    .Include(f=> f.Projects)
                    .FirstOrDefaultAsync(f => f.Id == id);
                if (freeelancer == null)
                {
                    apiResponse = new()
                    {
                        IsSuccess = false,
                        Error = new Error() { Code = 404, Message = $"Freelancer {id} Not Found." },
                    };
                    return NotFound(apiResponse);
                }

                freelancerDTO = new()
                {
                    Id = freeelancer.Id,
                    Name = freeelancer.User.Name,
                    Username = freeelancer.User.Username,
                    Skills = freeelancer.Skills,
                    Projects = freeelancer.Projects,
                };
            }
            else
            {
                apiResponse = new()
                {
                    IsSuccess = false,
                    Error = new Error() { Code= 400, Message = $"{ loadProjects }: is invalid loadProjectss value." }
                };
                return BadRequest(apiResponse);
            }
            apiResponse = new()
            {
                IsSuccess = true,
                Results = freelancerDTO
            };

            return Ok(apiResponse);
        }

        // Remove Freelancer by id
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFreelancerById(int id)
        {
            ApiResponseDTO <object> apiResponse;
            Freelancer? freelancer = await _mainAppContext.Freelancers.FindAsync(id);
            if(freelancer == null)
            {
                apiResponse = new()
                {
                    IsSuccess = false,
                    Error = new Error() { Code = 404, Message = $"Freelancer {id} Not Found." },
                };
                return NotFound(apiResponse);
            }
            _mainAppContext.Remove(freelancer);
            await _mainAppContext.SaveChangesAsync();
            apiResponse = new()
            {
                IsSuccess = true,
                Results = freelancer
            };
            return Ok(apiResponse);
        }

        // Updating Freelancer by Id
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFreelancerById(int id, [FromBody] FreelancerInputDTO freelancerDTO)
        {
            ApiResponseDTO<object> apiResponse;
            Freelancer? freelancer = await _mainAppContext.Freelancers
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (freelancer == null)
            {
                apiResponse = new()
                {
                    IsSuccess = false,
                    Error = new Error() { Code = 404, Message = $"Freelancer {id} Not Found." },
                };
                return NotFound(apiResponse);
            }
            // updating freelancer data
            freelancer.User.Name = freelancerDTO.Name; 
            freelancer.User.Username = freelancerDTO.Username;
            freelancer.User.Password = freelancerDTO.Password;
            freelancer.Skills = freelancerDTO.Skills;

            await _mainAppContext.SaveChangesAsync();
            // Creating freelancerOutDTO
            FreelancerOutDTO freelancerOutDTO = new()
            {
                Id = id,
                Name = freelancer.User.Name,
                Username = freelancer.User.Username,
                Skills = freelancer.Skills,
            };
            
            apiResponse = new()
            {
                IsSuccess = true,
                Results = freelancerOutDTO,
            };
            return Ok(apiResponse);
        }
    }
}
