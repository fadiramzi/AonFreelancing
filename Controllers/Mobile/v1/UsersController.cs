using AonFreelancing.Contexts;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Entities;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/users")]
    [ApiController]
    public class UsersController(MainAppContext mainAppContext)
        : BaseController
    {
        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetProfileByIdAsync([FromRoute]long id)
        {
            var freelancer = await mainAppContext.Users 
                .OfType<FreelancerEntity>().Where(f => f.Id == id)
                .Select(f => new FreelancerResponseDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    Email = f.Email ?? string.Empty,
                    PhoneNumber = f.PhoneNumber ?? string.Empty,
                    UserType = Constants.USER_TYPE_FREELANCER,
                    IsPhoneNumberVerified = f.PhoneNumberConfirmed,
                    Role = new RoleResponseDTO { Name = Constants.USER_TYPE_FREELANCER },
                    Skills = f.Skills,
                }).FirstOrDefaultAsync();


            if (freelancer != null)
                return Ok(CreateSuccessResponse(freelancer));
          
            var client = await mainAppContext.Users
                .OfType<ClientEntity>()
                .Where(c => c.Id == id)
                .Include(c => c.Projects)
                .Select(c => new ClientResponseDTO
                 {
                     Id = c.Id,
                     Name = c.Name,
                     Email = c.Email ?? string.Empty, 
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
