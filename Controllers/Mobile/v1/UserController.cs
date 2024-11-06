using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserController(MainAppContext mainAppContext, RoleManager<ApplicationRole> roleManager)
        {
            _mainAppContext = mainAppContext;
            _roleManager = roleManager;
        }

        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetProfileAysnc([FromRoute]long id)
        {


            var freelancer = await _mainAppContext.Users
                .OfType<Freelancer>().Where(f => f.Id == id)
                .Select(f => new FreelancerResponseDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    Username = f.UserName,
                    PhoneNumber = f.PhoneNumber,
                    UserType = Constants.USER_TYPE_FREELANCER,
                    IsPhoneNumberVerified = f.PhoneNumberConfirmed,
                    Role = new RoleResponseDTO { Name = Constants.USER_TYPE_FREELANCER },
                    Skills = f.Skills,
                }).FirstOrDefaultAsync();


            if (freelancer != null)
                return Ok(new ApiResponse<FreelancerResponseDTO>
                {
                    IsSuccess = true,
                    Results = freelancer,
                    Errors = null
                });


            var client = await _mainAppContext.Users
                .OfType<Client>()
                .Where(c => c.Id == id)
                .Select(c => new ClientResponseDTO
                 {
                     Id = c.Id,
                     Name = c.Name,
                     Username = c.UserName,
                     PhoneNumber = c.PhoneNumber,
                     UserType = Constants.USER_TYPE_CLIENT,
                     IsPhoneNumberVerified = c.PhoneNumberConfirmed,
                     Role = new RoleResponseDTO { Name = Constants.USER_TYPE_CLIENT },
                     CompanyName = c.CompanyName,

                 }).FirstOrDefaultAsync();


            if (client != null)
                return Ok(new ApiResponse<ClientResponseDTO>
                {
                    IsSuccess = true,
                    Results = client,
                    Errors = null
                });

            return NotFound(new ApiResponse<string>
            {
                IsSuccess = false,
                Results = null,
                Errors = new List<Error> {
                        new Error
                        {
                            Code = StatusCodes.Status404NotFound.ToString(),
                            Message = "User not found"
                        }
                    }
            });

        }
    }
   
}
