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
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController(MainAppContext mainAppContext, UserManager<User> userManager) : BaseController
    {
        [Authorize(Roles = "CLIENT, FREELANCER")]
        [HttpPut("tasks/{id}/updateStatus")]
        public async Task<IActionResult> UpdateTaskAsync(long id, [FromBody] TaskUpdateDTO taskUpdateDTO)
        {
            TaskEntity? storedTask = await mainAppContext.Tasks.FindAsync(id);
            if (storedTask != null && !storedTask.IsDeleted)
            {
                if (storedTask.Status == Constants.TASK_STATUS_DONE)
                    return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "task is already done"));

                if (taskUpdateDTO.Status == Constants.TASK_STATUS_DONE)
                    storedTask.CompletedAt = DateTime.Now;
                storedTask.Status = taskUpdateDTO.Status;
                storedTask.DeadlineAt = taskUpdateDTO.deadlineAt;
                storedTask.Name = taskUpdateDTO.Name;
                storedTask.Notes = taskUpdateDTO.notes;
                await mainAppContext.SaveChangesAsync();
                return Ok(CreateSuccessResponse("Task Has Been Updated "));
               
            }
            return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                   "Task not found."));
        }



        [Authorize(Roles = "CLIENT")]
         [HttpPut("tasks/{pid}/checkProgress")]
        public async Task<IActionResult> CheckProgressStatusAsync( int pid )
        {
            decimal countDone= await mainAppContext.Tasks.Where(s => s.Status== Constants.TASK_STATUS_DONE&&s.ProjectId==pid && s.IsDeleted==false).CountAsync();
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
