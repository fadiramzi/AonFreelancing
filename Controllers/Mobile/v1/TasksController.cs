using AonFreelancing.Contexts;
using AonFreelancing.Controllers;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
[Route("api/mobile/v1/tasks")]
[ApiController]
public class TasksController(MainAppContext mainAppContext,AuthService authService) : BaseController
{
    [Authorize(Roles = $"{Constants.USER_TYPE_CLIENT}, {Constants.USER_TYPE_FREELANCER}")]
    [HttpPatch("{id}/update-status")]
    public async Task<IActionResult> UpdateByIdAsync(long id, [FromBody] TaskStatusDto taskStatusDTO)
    {
        if (!ModelState.IsValid)
            return CustomBadRequest();
        long authenticatedUserId = authService.GetUserId((ClaimsIdentity) HttpContext.User.Identity);

        var storedTask = await mainAppContext.Tasks.Include(t=>t.Project)
                                                   .Where(t => t.Id == id && !t.IsDeleted)
                                                   .FirstOrDefaultAsync();
        if (storedTask == null)
            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "task not found"));
        if (authenticatedUserId != storedTask.Project.ClientId && authenticatedUserId != storedTask.Project.FreelancerId)
            return Forbid();
        if (taskStatusDTO.NewStatus == Constants.TASK_STATUS_DONE)
        {
            if (storedTask.Status == Constants.TASK_STATUS_DONE)
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "this task status is 'done' already"));
            if (storedTask.Status != Constants.TASK_STATUS_DONE)
                storedTask.CompletedAt = DateTime.Now;
        }
        
        storedTask.Status = taskStatusDTO.NewStatus;
        await mainAppContext.SaveChangesAsync();

        return Ok(CreateSuccessResponse(TaskOutputDTO.FromTask(storedTask)));
    }
    [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateByIdAsync(long id, [FromBody] TaskInputDTO taskInputDTO)
    {
        if (!ModelState.IsValid)
            return CustomBadRequest();
        long authenticatedClientId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
        TaskEntity? storedTask = await mainAppContext.Tasks.Include(t=>t.Project)
                                                           .Where(t => t.Id == id && !t.IsDeleted)
                                                           .FirstOrDefaultAsync();
        if (storedTask == null)
            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "task not found"));
        if (authenticatedClientId != storedTask.Project.ClientId )
            return Forbid();

        storedTask.UpdateFromInputDTO(taskInputDTO);
        await mainAppContext.SaveChangesAsync();

        return Ok(CreateSuccessResponse(TaskOutputDTO.FromTask(storedTask)));
    }

}