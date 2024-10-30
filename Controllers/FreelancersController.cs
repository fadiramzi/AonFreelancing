using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers
{
    [Route("api/v1/freelancers")]
    [ApiController]
    public class FreelancersController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;

        public FreelancersController(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            // entryPoint of DB comuniction
            var data = await _mainAppContext.Freelancers.Include(f => f.Projects).ToListAsync();
            return Ok(data);
        }
        //api/freelancers/
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Freelancer freelancer) 
        {
            await _mainAppContext.Freelancers.AddAsync(freelancer);
            await _mainAppContext.SaveChangesAsync(); 

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
        public async Task<IActionResult> GetFreelancer(int id, [FromQuery] FreelancerLoadProject loadProjects)
        {
            if (loadProjects != null) 
            {
               if (ModelState.IsValid)
                {
                    var fr = await _mainAppContext.Freelancers
                                    .Include(f => f.Projects)
                                    .Select(c => new FreelancerOutDTO
                                    {
                                        Id = c.Id,
                                        Skills = c.Skills,
                                        Name = c.Name,
                                        Username = c.Username
                                    })
                                    .Select(p => new ProjectOutDTO
                                    {
                                        Id = p.Id,
                                        Title = p.Title,
                                        Description = p.Description,
                                    })
                                    .FirstOrDefaultAsync(f => f.Id == id);

                    if (fr == null)
                    {
                        return NotFound("The resoucre is not found !");
                    }

                    return Ok(fr);
                }
                else
                {
                    return BadRequest("The query must be 0 or 1 !");
                }
            }
            else
            {
                return BadRequest("The query is mandatory !");
            }


        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            Freelancer? f = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f=>f.Id == id);
            if(f!= null)
            {
                _mainAppContext.Remove(f);
                await _mainAppContext.SaveChangesAsync();
                return Ok("Deleted");

            }

            return NotFound();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] Freelancer freelancer)
        {
            Freelancer? f = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
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
