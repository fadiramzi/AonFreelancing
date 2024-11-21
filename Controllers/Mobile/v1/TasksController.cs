using AonFreelancing.Contexts;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var storedTaskDTO = await mainAppContext.Tasks.AsNoTracking()
                                                        .Where(t => t.Id == id)
                                                        .Select(t => new TaskOutputDTO(t))
                                                        .FirstOrDefaultAsync();
            if (storedTaskDTO != null)
                return Ok(CreateSuccessResponse(storedTaskDTO));

            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Task not found"));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateByIdAsync(long id, [FromBody] TaskUpdateDTO taskUpdateDTO)
        {
            var storedTask = await mainAppContext.Tasks.Where(t => t.Id == id).FirstOrDefaultAsync();
            if (storedTask == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "invalid task id"));

            if (taskUpdateDTO.Status == Constants.TASK_STATUS_DONE)
                storedTask.CompletedAt = DateTime.Now;

            storedTask.Status = taskUpdateDTO.Status;
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse(new TaskOutputDTO(storedTask)));
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteById(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long clientId = Convert.ToInt64(identity.FindFirst(ClaimTypes.NameIdentifier).Value);
            Models.Task? storedTask = await mainAppContext.Tasks.Where(t => t.Id == id).FirstOrDefaultAsync();
            
            if (storedTask == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "invalid task id"));
            if (storedTask.ClientId != clientId)
                return StatusCode(StatusCodes.Status403Forbidden, CreateErrorResponse(StatusCodes.Status403Forbidden.ToString(), "you are not authorized to delete this task"));
            
            mainAppContext.Remove(storedTask);
            await mainAppContext.SaveChangesAsync();
            return Ok("Task deleted successfully");
        }
    }
}
