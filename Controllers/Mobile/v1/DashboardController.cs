using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Responses;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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
            var userType = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;



            if (userType == Constants.USER_TYPE_CLIENT)
            {
                var response = new DashboardResponse
                {
                    dashprojectsDTO = new DashprojectsDTO
                    {
                        Total = mainAppContext.Projects.Count(d => d.ClientId == user.Id),
                        Available = mainAppContext.Projects.Count(d => d.ClientId == user.Id && d.Status == Constants.PROJECT_STATUS_AVAILABLE),
                        Closed = mainAppContext.Projects.Count(d => d.ClientId == user.Id && d.Status == Constants.PROJECT_STATUS_CLOSED)
                    },
                    dashTasksDTO = new DashTasksDTO
                    {
                        Total = mainAppContext.Projects.Where(d => d.ClientId == user.Id).Include(d => d.Tasks).Count(),

                        ToDo = mainAppContext.Projects.Where(d => d.ClientId == user.Id).Include(d => d.Tasks)
                        .Where(d => d.Status == Constants.TASKS_STATUS_TO_DO).Count().ToString()+'%',

                        InProgress = mainAppContext.Projects.Where(d => d.ClientId == user.Id).Include(d => d.Tasks)
                        .Where(d => d.Status == Constants.TASKS_STATUS_IN_PROGRESS).Count().ToString() + '%',

                        InReview = mainAppContext.Projects.Where(d => d.ClientId == user.Id).Include(d => d.Tasks)
                        .Where(d => d.Status == Constants.TASKS_STATUS_IN_REVIEW).Count().ToString() + '%',

                        Done = mainAppContext.Projects.Where(d => d.ClientId == user.Id).Include(d => d.Tasks)
                        .Where(d => d.Status == Constants.TASKS_STATUS_DONE).Count().ToString() + '%',
                    }
                };
                return Ok(CreateSuccessResponse(response));
            }
            if (userType == Constants.USER_TYPE_FREELANCER)
            {
                var response = new DashboardResponse
                {
                    dashprojectsDTO = new DashprojectsDTO
                    {
                        Total = mainAppContext.Projects.Count(d => d.FreelancerId == user.Id),
                        Available = mainAppContext.Projects.Count(d => d.FreelancerId == user.Id && d.Status == Constants.PROJECT_STATUS_AVAILABLE),
                        Closed = mainAppContext.Projects.Count(d => d.FreelancerId == user.Id && d.Status == Constants.PROJECT_STATUS_CLOSED)
                    },
                    dashTasksDTO = new DashTasksDTO
                    {
                        Total = mainAppContext.Projects.Where(d => d.FreelancerId == user.Id).Include(d => d.Tasks).Count(),

                        ToDo = mainAppContext.Projects.Where(d => d.FreelancerId == user.Id).Include(d => d.Tasks)
                        .Where(d => d.Status == Constants.TASKS_STATUS_TO_DO).Count().ToString() + '%',

                        InProgress = mainAppContext.Projects.Where(d => d.FreelancerId == user.Id).Include(d => d.Tasks)
                        .Where(d => d.Status == Constants.TASKS_STATUS_IN_PROGRESS).Count().ToString() + '%',

                        InReview = mainAppContext.Projects.Where(d => d.FreelancerId == user.Id).Include(d => d.Tasks)
                        .Where(d => d.Status == Constants.TASKS_STATUS_IN_REVIEW).Count().ToString() + '%',

                        Done = mainAppContext.Projects.Where(d => d.FreelancerId == user.Id).Include(d => d.Tasks)
                        .Where(d => d.Status == Constants.TASKS_STATUS_DONE).Count().ToString() + '%',
                    }
                };
                return Ok(CreateSuccessResponse(response));
            }


            return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),"couldnt identify user."));
        }
    }
}

