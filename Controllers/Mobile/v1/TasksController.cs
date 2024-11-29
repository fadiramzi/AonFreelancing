using AonFreelancing.Contexts;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;


namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1")]
    [ApiController]
    public class TasksController(MainAppContext mainAppContext, UserManager<User> userManager) : BaseController
    {
        [Authorize(Roles = "CLIENT")]
        [HttpGet("{id}/tasks")]
        public async Task<IActionResult> GetTaskAsync(long id, [FromQuery] string? status = Constants.TASKS_STATUS_TO_DO)
        {
            var project = await mainAppContext.Projects.FindAsync(id);

            if (project == null || project.Status != Constants.PROJECT_STATUS_CLOSED)
            {
                return BadRequest(CreateErrorResponse("400", "Project not found or not closed."));
            }

            IQueryable<TaskEntity> tasksQuery = mainAppContext.Tasks.Where(t => t.ProjectId == id);

            if (!string.IsNullOrEmpty(status))
            {
                tasksQuery = tasksQuery.Where(t => t.Status.ToLower() == status.ToLower());
            }

            var tasks = await tasksQuery.ToListAsync();

            if (tasks.Count == 0)
            {
                return NotFound(CreateErrorResponse("404", "No tasks found."));
            }

            return Ok(CreateSuccessResponse(tasks));
        }



        [Authorize(Roles = "CLIENT, FREELANCER")]
        [HttpPut("tasks/{id}/updateStatus")]
        public async Task<IActionResult> UpdateTaskAsync(long id, [FromBody] TaskUpdateDTO taskUpdateDTO)
        {
            //get task first and check its status if exist
            var task = await mainAppContext.Tasks.FindAsync(id);
            if (task != null && !task.IsDeleted)
            {
                //if its already done cant make any change to it 
                if (task.Status == Constants.TASKS_STATUS_DONE)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Results = "task is already done",
                    });
                }
                //update task status 
                //if new status is done we should update completedAt field too
                if (taskUpdateDTO.Status == Constants.TASKS_STATUS_DONE)
                    task.CompletedAt = DateTime.Now;
                task.Status = taskUpdateDTO.Status;
                task.DeadlineAt = taskUpdateDTO.deadlineAt;
                task.Name = taskUpdateDTO.Name;
                task.Notes = taskUpdateDTO.notes;
                await mainAppContext.SaveChangesAsync();
                return Ok(CreateSuccessResponse("Task Has Been Updated "));
               
            }
            return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                   "Task not found."));
        }



        [Authorize(Roles = "CLIENT")]
         [HttpGet("tasks/{pid}/checkProgress")]
        public async Task<IActionResult> CheckProgressStatusAsync( int pid )
        {
            decimal countDone= await mainAppContext.Tasks.Where(s => s.Status== Constants.TASKS_STATUS_DONE&&s.ProjectId==pid && s.IsDeleted==false).CountAsync();
            decimal countTotal = await mainAppContext.Tasks.Where(s => s.ProjectId == pid && s.IsDeleted == false ).CountAsync();
            if (countTotal > 0) 
            {
                int progress = (int)Math.Round(countDone / countTotal * 100);

                return Ok(CreateSuccessResponse(progress));

            }
            return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "project has no tasks"));
        }

        [Authorize(Roles = "CLIENT, FREELANCER")]
        [HttpPut("tasks/{id}/deleteTask")]
        public async Task<IActionResult> DeleteTaskAsync(long id)
        {
            //get task first and check its status if exist
            var task = await mainAppContext.Tasks.FindAsync(id);
            if (task != null)
            {

                //delete task status 
                //if status is deleted we should update DeletedAt and IsDeleted field too

                task.IsDeleted = true;
                task.DeletedAt = DateTime.Now;
                await mainAppContext.SaveChangesAsync();
                return Ok(CreateSuccessResponse("Task Has Been deleted "));

            }
            return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                   "Task not found."));


        }
    }
}
