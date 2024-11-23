using AonFreelancing.Contexts;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/tasks")]
    [ApiController]
    public class TasksController(MainAppContext mainAppContext) : BaseController
    {
        [Authorize(Roles = "CLIENT, FREELANCER")]
        [HttpPatch("{id}/update-status")]
        public async Task<IActionResult> UpdateByIdAsync(long id, [FromBody] TaskStatusDto taskStatusDTO)
        {
            var storedTask = await mainAppContext.Tasks.Where(t => t.Id == id).FirstOrDefaultAsync();
            if (storedTask == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "invalid task id"));

            if (taskStatusDTO.NewStatus == "Done")
                storedTask.CompletedAt = DateTime.Now;
            if (storedTask.Status == "Done" && taskStatusDTO.NewStatus != "Done")
                storedTask.CompletedAt = null;
            storedTask.Status = taskStatusDTO.NewStatus;
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse(new TaskOutputDTO(storedTask)));
        }
        [Authorize(Roles = "CLIENT")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateByIdAsync(long id, [FromBody] TaskInputDto taskInputDTO)
        {
            var storedTask = await mainAppContext.Tasks.Where(t => t.Id == id).FirstOrDefaultAsync();
            if (storedTask == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "invalid task id"));

            storedTask.Name= taskInputDTO.Name;
            storedTask.Notes = taskInputDTO.Notes;
            storedTask.DeadlineAt= taskInputDTO.DeadlineAt;

            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse(new TaskOutputDTO(storedTask)));
        }
        
    }
}