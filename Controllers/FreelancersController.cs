using System.ComponentModel.DataAnnotations;
using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
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
        public async Task<IActionResult> GetAllAsync()
        {
            var freelancers = await _mainAppContext.Freelancers
                .Include(f => f.Projects)
                .Select(f => new FreelancerDTO
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
                        ClientId = p.ClientId,
                        FreelancerId = p.FreelancerId,
                        CreatedAt = p.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            var response = new ApiResponse<List<FreelancerDTO>>()
            {
                IsSuccess = true,
                Results = freelancers
            };

            return Ok(response);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreelancerAsync(int id,
            [FromQuery] [Range(0, 1, ErrorMessage = "Can not load project")] int loadProjects)
        {
            var response = new ApiResponse<FreelancerDTO>();

            if (!ModelState.IsValid || (loadProjects != 0 && loadProjects != 1))
            {
                response.IsSuccess = false;
                response.Error = new Error
                {
                    Code = "400",
                    Message = "Invalid value for 'loadProjects'. Must be 0 or 1."
                };
                return BadRequest(response);
            }

            try
            {
                Freelancer? freelancer;

                if (loadProjects == 0)
                {
                    freelancer = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
                }
                else
                {
                    freelancer = await _mainAppContext.Freelancers
                        .Include(f => f.Projects)
                        .FirstOrDefaultAsync(f => f.Id == id);
                }

                if (freelancer == null)
                {
                    response.IsSuccess = false;
                    response.Error = new Error
                    {
                        Code = "404",
                        Message = "Freelancer not found."
                    };
                    return NotFound(response);
                }

                var freelancerDto = new FreelancerDTO
                {
                    Id = freelancer.Id,
                    Name = freelancer.Name,
                    Username = freelancer.Username,
                    Skills = freelancer.Skills,
                    Projects = loadProjects == 1
                        ? freelancer.Projects.Select(p => new ProjectOutDTO
                        {
                            Id = p.Id,
                            Title = p.Title,
                            Description = p.Description,
                            ClientId = p.ClientId,
                            CreatedAt = p.CreatedAt
                        }).ToList()
                        : new List<ProjectOutDTO>()
                };

                response.IsSuccess = true;
                response.Results = freelancerDto;


                return Ok(response);
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Error = new Error
                {
                    Code = "500",
                    Message = e.Message
                };
                return StatusCode(500, response);
            }
        }


        [HttpPost("RegisterFreelancer")]
        public async Task<IActionResult> RegisterAsync([FromBody] FreelancerInputDTO freelancerInputDto)
        {
            var response = new ApiResponse<FreelancerDTO>();
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "422",
                    Message = "Invalid data",
                };

                return UnprocessableEntity(response);
            }

            try
            {
                var freelancer = new Freelancer()
                {
                    Name = freelancerInputDto.Name,
                    Username = freelancerInputDto.Username,
                    Password = freelancerInputDto.Password,
                    Skills = freelancerInputDto.Skills,
                };

                _mainAppContext.Freelancers.Add(freelancer);
                await _mainAppContext.SaveChangesAsync();

                var freelancerDto = new FreelancerDTO
                {
                    Id = freelancer.Id,
                    Name = freelancer.Name,
                    Username = freelancer.Username,
                    Skills = freelancer.Skills,
                    Projects = new List<ProjectOutDTO>()
                };

                response.IsSuccess = true;
                response.Results = freelancerDto;

                return CreatedAtAction(nameof(GetFreelancerAsync), new { id = freelancerDto.Id }, response);
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "500",
                    Message = e.Message,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] FreelancerInputDTO freelancerInputDto)
        {
            
            var response = new ApiResponse<FreelancerDTO>();

            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.Error = new Error
                {
                    Code = "400",
                    Message = "Invalid input data"
                };
                return BadRequest(response);
            }
            
            try
            {
                var freelancer = await _mainAppContext.Freelancers
                    .Include(c => c.Projects)
                    .FirstOrDefaultAsync(c => c.Id == id);
                if (freelancer == null)
                {
                    response.IsSuccess = false;
                    response.Error = new Error()
                    {
                        Code = "404",
                        Message = "Freelancer not found",
                    };
                }

                freelancer!.Name = freelancerInputDto.Name;
                freelancer.Username = freelancerInputDto.Username;
                freelancer.Password = freelancerInputDto.Password;
                freelancer.Skills = freelancerInputDto.Skills;

                await _mainAppContext.SaveChangesAsync();

                
                    var updateFreelancerDto = new FreelancerDTO()
                    {
                        Id = freelancer.Id,
                        Name = freelancer.Name,
                        Username = freelancer.Username,
                        Skills = freelancer.Skills,
                        Projects = freelancer.Projects.Select(p => new ProjectOutDTO()
                        {
                            Id = p.Id,
                            Title = p.Title,
                            Description = p.Description,
                            ClientId = p.ClientId,
                            FreelancerId = freelancer.Id,
                            CreatedAt = p.CreatedAt,
                        })
                    };

                    response.IsSuccess = true;
                    response.Results = updateFreelancerDto;

                return Ok(response);
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "500",
                    Message = e.Message,
                };
                return StatusCode(500 ,response);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var response = new ApiResponse<string>();

            try
            {
                var freelancer = await _mainAppContext.Freelancers
                    .FirstOrDefaultAsync(f => f.Id == id);
                if (freelancer == null)
                {
                    response.IsSuccess = false;
                    response.Error = new Error()
                    {
                        Code = "404",
                        Message = "Freelancer not found",
                    };
                    return NotFound(response);
                }
                
                _mainAppContext.Freelancers.Remove(freelancer);
                await _mainAppContext.SaveChangesAsync();
                
                response.IsSuccess = true;
                response.Results = "Freelancer was successfully deleted.";
                
                return Ok(response);
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.Error = new Error()
                {
                    Code = "500",
                    Message = e.Message,
                };
                return StatusCode(500 ,response);
            }
        }
    }
}