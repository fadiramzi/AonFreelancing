using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsController (MainAppContext mainAppContext): BaseController
    {
        [Authorize(Roles =Constants.USER_TYPE_FREELANCER)]
        [HttpPost]
        public async Task<IActionResult> CreateSkill([FromBody] SkillInputDTO skillInputDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long authenticatedUserId = Convert.ToInt64(identity.FindFirst(ClaimTypes.NameIdentifier).Value);

            Skill? newSkill = Skill.FromInputDTO(skillInputDTO,authenticatedUserId);
            await mainAppContext.Skills.AddAsync(newSkill);
            await mainAppContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created,"skill added successfully");
        }

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSkill(long id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long authenticatedUserId = Convert.ToInt64(identity.FindFirst(ClaimTypes.NameIdentifier).Value);

            Skill? storedSkill= mainAppContext.Skills.Where(s=>s.Id == id).FirstOrDefault();
                if (storedSkill == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Skill not found"));
            if (storedSkill.UserId != authenticatedUserId)
                return Forbid();

            mainAppContext.Skills.Remove(storedSkill);
            await mainAppContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
