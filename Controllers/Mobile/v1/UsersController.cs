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
    public class UsersController(MainAppContext mainAppContext, RoleManager<ApplicationRole> roleManager)
        : BaseController
    {
        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetProfileByIdAsync([FromRoute]long id)
        {
            var storedFreelancer = await mainAppContext.Users 
                .OfType<Freelancer>().Where(f => f.Id == id)
                .Select(f => new FreelancerResponseDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    Username = f.UserName ?? string.Empty,
                    PhoneNumber = f.PhoneNumber ?? string.Empty,
                    UserType = Constants.USER_TYPE_FREELANCER,
                    IsPhoneNumberVerified = f.PhoneNumberConfirmed,
                    Role = new RoleResponseDTO { Name = Constants.USER_TYPE_FREELANCER },
                    Skills = f.Skills,
                }).FirstOrDefaultAsync();


            if (storedFreelancer != null)
                return Ok(new ApiResponse<FreelancerResponseDTO>
                {
                    IsSuccess = true,
                    Results = storedFreelancer,
                    Errors = null
                });


            var storedClient = await mainAppContext.Users
                .OfType<Client>()
                .Where(c => c.Id == id)
                .Include(c => c.Projects)
                .Select(c => new ClientResponseDTO
                 {
                     Id = c.Id,
                     Name = c.Name,
                     Username = c.UserName ?? string.Empty, 
                     PhoneNumber = c.PhoneNumber ?? string.Empty,
                     UserType = Constants.USER_TYPE_CLIENT,
                     IsPhoneNumberVerified = c.PhoneNumberConfirmed,
                     Role = new RoleResponseDTO { Name = Constants.USER_TYPE_CLIENT },
                     Projects = c.Projects.Select(p =>  new ProjectDetailsDTO
                     {
                         Id = p.Id,
                         Description = p.Description,
                         EndDate = p.EndDate,
                         StartDate = p.StartDate,
                         Name = p.Title,
                     }),
                     CompanyName = c.CompanyName,

                 }).FirstOrDefaultAsync();


            if (storedClient != null)
                return Ok(CreateSuccessResponse(storedClient));

            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "NotFound"));

        }
    }
   
}
