using AonFreelancing.Contexts;
using AonFreelancing.Interfaces;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization.Formatters;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [ApiController]
    [Route("api/mobile/v1")]
    public class DashboardController : BaseController
    {
        private readonly MainAppContext _mainAppContext;
        private readonly UserManager<User> _userManager;
        private readonly IStatisticsService _statisticsService;

        public DashboardController(MainAppContext mainAppContext, IStatisticsService statisticsService, UserManager<User> userManager)
        {
            _mainAppContext = mainAppContext;
            _statisticsService = statisticsService;
            _userManager = userManager;
        }

        [HttpGet("GetMyStatistics")]
        public async Task<IActionResult> GetMyStatistics()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Unable to load user."));

            var userId = user.Id;
            
            var projects = await _mainAppContext.Projects
                .Where(p => p.FreelancerId == userId || p.ClientId == userId)
                .ToListAsync();

            var tasks = await _mainAppContext.Tasks
                    .Where(t => !t.IsDeleted && projects.Select(p => p.Id).Contains(t.ProjectId))
                    .ToListAsync();

            // Project statistics
            int totalProjects = projects.Count();
            int availableProjects = projects.Count(p => p.Status == Constants.PROJECT_STATUS_AVAILABLE);
            int closedProjects = projects.Count(p => p.Status == Constants.PROJECT_STATUS_CLOSED);

            // Skill issues here (Diyar)
            //int totalTasks = tasks.Count;
            //int toDoTasks = tasks.Count(t => t.Status.Trim().Equals(Constants.TASKS_STATUS_TO_DO, StringComparison.OrdinalIgnoreCase));
            //int inProgressTasks = tasks.Count(t => t.Status.Trim().Equals(Constants.TASKS_STATUS_IN_PROGRESS, StringComparison.OrdinalIgnoreCase));
            //int inReviewTasks = tasks.Count(t => t.Status.Trim().Equals(Constants.TASKS_STATUS_IN_REVIEW, StringComparison.OrdinalIgnoreCase));
            //int doneTasks = tasks.Count(t => t.Status.Trim().Equals(Constants.TASKS_STATUS_DONE, StringComparison.OrdinalIgnoreCase));

            // Task statistics
            int totalTasks = tasks.Count;
            int toDoTasks = tasks.Count(t => t.Status == Constants.TASKS_STATUS_TO_DO);
            int inProgressTasks = tasks.Count(t => t.Status == Constants.TASKS_STATUS_IN_PROGRESS);
            int inReviewTasks = tasks.Count(t => t.Status == Constants.TASKS_STATUS_IN_REVIEW);
            int doneTasks = tasks.Count(t => t.Status == Constants.TASKS_STATUS_DONE);


            // Prepare response
            var response = new StatisticsResponseDTO
            {
                Projects = new ProjectStatistics
                {
                    Total = totalProjects,
                    Available = availableProjects,
                    Closed = closedProjects
                },
                Tasks = new TaskStatistics
                {
                    Total = totalTasks,
                    ToDo = _statisticsService.CalculatePercentage(totalTasks, toDoTasks),
                    InProgress = _statisticsService.CalculatePercentage(totalTasks, inProgressTasks),
                    InReview = _statisticsService.CalculatePercentage(totalTasks, inReviewTasks),
                    Done = _statisticsService.CalculatePercentage(totalTasks, doneTasks)
                }
            };

            return Ok(CreateSuccessResponse(response));
        }

    }
}
