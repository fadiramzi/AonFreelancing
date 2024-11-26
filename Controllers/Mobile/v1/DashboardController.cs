using AonFreelancing.Contexts;
using AonFreelancing.Interfaces;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [ApiController]
    [Route("api/mobile/v1")]
    public class DashboardController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;
        //private readonly UserManager<User> _userManager;
        private readonly IStatisticsService _statisticsService;

        public DashboardController(MainAppContext mainAppContext, IStatisticsService statisticsService, UserManager<User> userManager)
        {
            _mainAppContext = mainAppContext;
            _statisticsService = statisticsService;
            //_userManager = userManager;
        }

        [HttpGet("GetMyStatistics")]
        public async Task<IActionResult> GetMyStatistics()
        {
            try
            {
                var projects = await _mainAppContext.Projects.ToListAsync();
                var tasks = await _mainAppContext.Tasks.Where(t => !t.IsDeleted).ToListAsync(); // Checking

                // Project statistics
                int totalProjects = projects.Count();
                int availableProjects = projects.Count(p => p.Status == Constants.PROJECT_STATUS_AVAILABLE);
                int closedProjects = projects.Count(p => p.Status == Constants.PROJECT_STATUS_CLOSED);

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

                // Return success response
                var apiResponse = new ApiResponse<StatisticsResponseDTO>
                {
                    IsSuccess = true,
                    Results = response,
                    Errors = null,
                    Message = "Statistics retrieved successfully."
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // Handle exceptions and return an error response
                var apiResponse = new ApiResponse<StatisticsResponseDTO>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error>
            {
                new Error
                {
                    Code = "500",
                    Message = ex.Message
                }
            },
                    Message = "An error occurred while fetching statistics."
                };

                return StatusCode(500, apiResponse);  // Internal Server Error
            }
        }

    }
}
