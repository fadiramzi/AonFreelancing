using System.Diagnostics.Eventing.Reader;
using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AonFreelancing.Utilities;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Route("api/mobile/v1/users")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;
        private readonly UserManager<User> _userManager;
        public UsersController(
            MainAppContext mainAppContext,
            UserManager<User> userManager
            )
        {
            _mainAppContext = mainAppContext;
            _userManager = userManager;
        }        

        [HttpGet("/{id}/profile")]
        public async Task<IActionResult> GetUserById(long id)
        {
            //var user = await _userManager.FindByIdAsync(id);
            var freelancer = await _mainAppContext.Users.OfType<Freelancer>().FirstOrDefaultAsync(f => f.Id == id);
            var client = await _mainAppContext.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == id);

            if (freelancer is not null)
            {
                return Ok(new ApiResponse<UserResponseDTO>()
                {
                    IsSuccess = true,
                    Results = new FreelancerResponseDTO(){ 
                        Id = freelancer.Id,
                        Name = freelancer.Name,
                        Username = freelancer.UserName,
                        PhoneNumber = freelancer.PhoneNumber,
                        IsPhoneNumberVerified = freelancer.PhoneNumberConfirmed,
                        UserType = Constants.USER_TYPE_FREELANCER,
                        Skills = freelancer.Skills,
                    },
                    Errors = []
                });
            }
            else if (client is not null)
            {
                return Ok(new ApiResponse<ClientResponseDTO>()
                {
                    IsSuccess = true,
                    Results = new ClientResponseDTO()
                    {
                        Id = client.Id,
                        Name = client.Name,
                        Username = client.UserName,
                        PhoneNumber = client.PhoneNumber,
                        IsPhoneNumberVerified = client.PhoneNumberConfirmed,
                        UserType = Constants.USER_TYPE_CLIENT,
                        CompanyName = client.CompanyName
                    },
                    Errors = []
                });
            }
            // User doesn't exisit as a Freelancer or Client.
            return NotFound(new ApiResponse<User>()
            {
                IsSuccess = false,
                Errors = [new Error()
                {
                    Code= "404",
                    Message = $"user with {id} id is Not Found."
                }]
            });
        }
    }
}
