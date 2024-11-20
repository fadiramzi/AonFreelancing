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
<<<<<<< HEAD
        private readonly MainAppContext _mainAppContext;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public UsersController(MainAppContext mainAppContext, RoleManager<ApplicationRole> roleManager)
        {
            _mainAppContext = mainAppContext;
            _roleManager = roleManager;
        }

        [Authorize]
=======
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2
        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetProfileByIdAsync([FromRoute]long id)
        {
<<<<<<< HEAD
            var freelancerResponseDTO = await _mainAppContext.Users.OfType<Freelancer>()
                                                                .Where(f => f.Id == id)
                                                                .Select(f => new FreelancerResponseDTO
                                                                {
                                                                    Id = f.Id,
                                                                    Name = f.Name,
                                                                    Username = f.UserName,
                                                                    PhoneNumber = f.PhoneNumber,
                                                                    UserType = Constants.USER_TYPE_FREELANCER,
                                                                    IsPhoneNumberVerified = f.PhoneNumberConfirmed,
                                                                    Role = new RoleResponseDTO { Id = f.Id, Name = Constants.USER_TYPE_FREELANCER },
                                                                    About = f.About,
                                                                    Skills = f.Skills,
                                                                }).FirstOrDefaultAsync();
            if (freelancerResponseDTO != null)
                return Ok(CreateSuccessResponse(freelancerResponseDTO));


            var clientResponseDTO = await _mainAppContext.Users.OfType<Client>()
                                                                .Where(c => c.Id == id)
                                                                .Include(c => c.Projects)
                                                                .Select(c => new ClientResponseDTO
                                                                {
                                                                    Id = c.Id,
                                                                    Name = c.Name,
                                                                    Username = c.UserName,
                                                                    PhoneNumber = c.PhoneNumber,
                                                                    UserType = Constants.USER_TYPE_CLIENT,
                                                                    IsPhoneNumberVerified = c.PhoneNumberConfirmed,
                                                                    Role = new RoleResponseDTO { Id = c.Id, Name = Constants.USER_TYPE_CLIENT },
                                                                    About = c.About,
                                                                    CompanyName = c.CompanyName,
                                                                    Projects = c.Projects
                                                                }).FirstOrDefaultAsync();
=======
            var freelancer = await mainAppContext.Users 
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


            if (freelancer != null)
                return Ok(new ApiResponse<FreelancerResponseDTO>
                {
                    IsSuccess = true,
                    Results = freelancer,
                    Errors = null
                });
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2


            var client = await mainAppContext.Users
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


            if (client != null)
                return Ok(CreateSuccessResponse(client));

            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "NotFound"));

        }
    }
   
}
