using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/freelancers")]
    [ApiController]
    public class FreelancersController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly MainAppContext _mainAppContext;

        public FreelancersController(
            MainAppContext mainAppContext,
            UserManager<User> userManager
            )
        {
            _mainAppContext = mainAppContext;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            // entryPoint of DB comuniction

            var data = await _mainAppContext.Users.OfType<Freelancer>()
                .Select(u=>new FreelancerResponseDTO() {
                    Id = u.Id,
                    Name = u.Name,
                    PhoneNumber = u.PhoneNumber,
                    UserType = Constants.USER_TYPE_FREELANCER,
                    Skills = u.Skills,
                })
                .ToListAsync();
            return Ok(CreateSuccessResponse(data));
        }
        //api/freelancers/
        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] FreelancerRequestDTO freelancer) {

        //    Freelancer f = new Freelancer()
        //    {
        //        Name = freelancer.Name,
        //        PhoneNumber = freelancer.PhoneNumber,
        //        UserName = freelancer.Username,
        //        Skills = "Programming, net Core"
        //    };
        //    await _userManager.CreateAsync(f,freelancer.Password);

        //    return CreatedAtAction("Create", new { Id = f.Id }, f);
        //}

        ////api/freelancers/Register
        //[HttpPost("Register")]
        //public async Task<IActionResult> Register([FromBody] FreelancerDTO freelancerDTO)
        //{
        //    ApiResponse<object> apiResponse;

        //    Freelancer f = new Freelancer();
        //    f.Name = freelancerDTO.Name;
        //    f.UserName = freelancerDTO.Username;
        //    f.PasswordHash = freelancerDTO.Password;
        //    f.Skills = freelancerDTO.Skills;

        //    await _mainAppContext.Freelancers.AddAsync(f);
        //    await _mainAppContext.SaveChangesAsync();
        //    apiResponse = new ApiResponse<object>
        //    {
        //        IsSuccess = true,
        //        Results = f
        //    };


        //    return Ok(apiResponse);
        //}

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetFreelancer(int id)
        //{

        //    Freelancer? fr = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);

        //    if (fr == null)
        //    {
        //        return NotFound("The resoucre is not found!");
        //    }

        //    return Ok(fr);

        //}

        //[HttpDelete("{id}")]
        //public IActionResult Delete(int id)
        //{
        //    Freelancer f = _mainAppContext.Freelancers.FirstOrDefault(f=>f.Id == id);
        //    if(f!= null)
        //    {
        //        _mainAppContext.Remove(f);
        //        _mainAppContext.SaveChanges();
        //        return Ok("Deleted");

        //    }

        //    return NotFound();
        //}

        //[HttpPut("{id}")]
        //public IActionResult Update(int id, [FromBody] Freelancer freelancer)
        //{
        //    Freelancer f = _mainAppContext.Freelancers.FirstOrDefault(f => f.Id == id);
        //    if (f != null)
        //    {
        //        f.Name = freelancer.Name;

        //        _mainAppContext.SaveChanges();
        //        return Ok(f);

        //    }

        //    return NotFound();
        //}



    }
}
