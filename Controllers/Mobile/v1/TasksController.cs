using AonFreelancing.Contexts;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/tasks")]
    [ApiController]
    public class TasksController(MainAppContext mainAppContext, UserManager<User> userManager) : BaseController
    {
        [Authorize(Roles = "CLIENT, FREELANCER")]
        [HttpPut("{id}/updateStatus")]
        public async Task<IActionResult> UpdateTaskStatusAsync(int id, [FromBody] TaskStatusDto taskStatusDto)
        {
            var task = await mainAppContext.Tasks.FindAsync(id);
            if (task == null)
            {
                var errorResponse = new ApiResponse<string>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error>
                {
                    new Error { Code = "404", Message = "Task not found." }
                }
                };
                return NotFound(errorResponse);
            }

            var validStatuses = new List<string> { "to-do", "in-progress", "in-review", "done" };

            if (!validStatuses.Contains(taskStatusDto.NewStatus.ToLower()))
            {
                var errorResponse = new ApiResponse<string>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error>
                {
                    new Error { Code = "400", Message = "Invalid status provided." }
                }
                };
                return BadRequest(errorResponse);
            }

            if (task.Status.ToLower() == "to do" && taskStatusDto.NewStatus.ToLower() != "in progress")
            {
                var errorResponse = new ApiResponse<string>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error>
                {
                    new Error { Code = "400", Message = "Invalid status transition from 'To Do'." }
                }
                };
                return BadRequest(errorResponse);
            }
            if (task.Status.ToLower() == "in progress" &&
                taskStatusDto.NewStatus.ToLower() != "in review" && taskStatusDto.NewStatus.ToLower() != "done")
            {
                var errorResponse = new ApiResponse<string>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error>
                {
                    new Error { Code = "400", Message = "Invalid status transition from 'In Progress'." }
                }
                };
                return BadRequest(errorResponse);
            }
            if (task.Status.ToLower() == "in review" && taskStatusDto.NewStatus.ToLower() != "done")
            {
                var errorResponse = new ApiResponse<string>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error>
                {
                    new Error { Code = "400", Message = "Invalid status transition from 'In Review'." }
                }
                };
                return BadRequest(errorResponse);
            }
            if (task.Status.ToLower() == "done")
            {
                var errorResponse = new ApiResponse<string>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error>
                {
                    new Error { Code = "400", Message = "No further status transitions allowed from 'Done'." }
                }
                };
                return BadRequest(errorResponse);
            }

            task.Status = taskStatusDto.NewStatus;

            task.CompletedAt = taskStatusDto.NewStatus.ToLower() == "done" ? DateTime.UtcNow : (DateTime?)null;

            await mainAppContext.SaveChangesAsync();

            var successResponse = new ApiResponse<string>
            {
                IsSuccess = true,
                Results = "Task status updated.",
                Errors = null
            };
            return Ok(successResponse);
        }

        // api/mobile/v1/tasks/{id} PUT
        [Authorize(Roles = "CLIENT")]
        [HttpPut("tasks/{id}")]
        public async Task<IActionResult> UpdateTaskDetailsAsync(int id, [FromBody] TaskDetailsDto taskDto)
        {
            var task = await mainAppContext.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Errors = new List<Error> { new Error { Code = "404", Message = "Task not found." } }
                });
            }

            task.Name = taskDto.Name;
            task.DeadlineAt = taskDto.DeadlineAt;
            task.Notes = taskDto.Notes;

            await mainAppContext.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                IsSuccess = true,
                Results = "Task details updated."
            });
        }

        // api/mobile/v1/tasks/{id} DELETE
        [HttpDelete("tasks/{id}")]
        public async Task<IActionResult> DeleteTaskAsync(int id)
        {
            var task = await mainAppContext.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Errors = new List<Error> { new Error { Code = "404", Message = "Task not found." } }
                });
            }

            mainAppContext.Tasks.Remove(task);
            await mainAppContext.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                IsSuccess = true,
                Results = "Task deleted successfully."
            });
        }

    }
}
