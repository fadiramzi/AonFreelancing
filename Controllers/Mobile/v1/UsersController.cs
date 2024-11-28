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
    public class UsersController(MainAppContext mainAppContext, UserManager<User> userManager, RoleManager<ApplicationRole> roleManager)
        : BaseController
    {
        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetProfileByIdAsync([FromRoute]long id)
        {
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
                    Skills = f.Skills.Select(p => new SkillDTO
                    {
                        Name = p.Name
                    }),
                }).FirstOrDefaultAsync();


            if (freelancer != null)
                return Ok(new ApiResponse<FreelancerResponseDTO>
                {
                    IsSuccess = true,
                    Results = freelancer,
                    Errors = null
                });


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

        

        [Authorize(Roles = "FREELANCER")]
        [HttpPost("{id}/skills")]
        public async Task<IActionResult> UpdateTaskAsync(long id, [FromBody] SkillDTO skillDTO)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Unable to set skills."));
            if (user.Id != id)
                return BadRequest(CreateErrorResponse(StatusCodes.Status403Forbidden.ToString(),
                    "Not alowed"));


            if (!ModelState.IsValid)
            {

                return base.CustomBadRequest();
            }

            var freelancer = await mainAppContext.Users.OfType<Freelancer>().FirstOrDefaultAsync(f => f.Id == id);
            if (freelancer == null)
            {
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "freelancer not found."));
            }
            var freelancerSkill = await mainAppContext.skills.Where(s => s.UserId == id && s.Name == skillDTO.Name).FirstOrDefaultAsync();
            if (freelancerSkill != null)
            {
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "skill aliready exist."));
            }
            var skill = new Skill
            {
                UserId = id,
                Name = skillDTO.Name,
            };
            await mainAppContext.skills.AddAsync(skill);
            await mainAppContext.SaveChangesAsync();
            return Ok(CreateSuccessResponse("skill Has Been added "));

        }

        [Authorize(Roles = "FREELANCER")]
        [HttpDelete("{id}/skills")]
        public async Task<IActionResult> DeleteTaskAsync(long id, [FromBody] SkillDTO skillDTO)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Unable to delete skills."));
            if (user.Id != id)
                return BadRequest(CreateErrorResponse(StatusCodes.Status403Forbidden.ToString(),
                    "Not alowed"));

            var skill = await mainAppContext.skills.Where(s => s.Name == skillDTO.Name && s.UserId == id).FirstOrDefaultAsync();
            if (skill != null)
            {
                mainAppContext.skills.Remove(skill);
                await mainAppContext.SaveChangesAsync();
                return Ok(CreateSuccessResponse("skill Has Been deleted "));

            }
            return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                   "skill not found."));


        }
    }
   
}
