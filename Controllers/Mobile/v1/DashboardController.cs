using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static AonFreelancing.Models.DTOs.DashboardDTO;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController(MainAppContext mainAppContext, UserManager<User> userManager, RoleManager<ApplicationRole> roleManager)
        : BaseController
    {
        [Authorize(Roles = "CLIENT,FREELANCER")]
        [HttpGet("GetMyStatistics")]
        public async Task<IActionResult> GetMyStatisticsAsync()
        {

            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Unable to set skills."));

            var claimList = await userManager.GetClaimsAsync(user);
            var userClaim =claimList.FirstOrDefault().ToString();
            if (userClaim == Constants.USER_TYPE_CLIENT)
            {
                //DashProjects dashProjects = new DashProjects
                //{
                //    dashProjects.Total = mainAppContext.Projects.Count(d => d.ClientId == user.Id),

                //};
            }

            return BadRequest();
        }
    }
}
