using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Route("api/tasks")]
    [ApiController]
    [Authorize]
    public class TasksController : BaseController
    {
        private readonly MainAppContext _mainAppContext;

        public TasksController(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] UpdateTaskStatusDTO updateTaskStatusDTO)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            var task = await _mainAppContext.Tasks.FirstOrDefaultAsync(t => t.Id ==id);
            if (task == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Task is not found !"));

            task.Status = updateTaskStatusDTO.NewStatus;

            // if task is completed then assigen completedAt
            if (updateTaskStatusDTO.NewStatus == "done")
                task.CompletedAt = DateTime.Now;

            _mainAppContext.Tasks.Update(task);
            await _mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Task updated successfully"));
        }
    }
}
