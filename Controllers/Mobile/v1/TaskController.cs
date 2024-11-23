using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/Task")]
    [ApiController]
    public class TaskController(MainAppContext mainAppContext, UserManager<User> userManager,TaskService taskService) : BaseController
    {



        [HttpPut("{id}/UpdateTask")]
        public async Task<IActionResult>UpdateTask(int id, [FromBody]TaskInputDto T)
        {

            if (!ModelState.IsValid) 
                return BadRequest(CreateErrorResponse("400","enter vaild Input"));
            
            var task=await mainAppContext .Tasks.Where(t=>t.Id==id).FirstOrDefaultAsync();
            if (task != null) { 
           task.Name = T.Name;
                task.DeadlineAt=T.DeadlineAt;
                task.Notes = T.Notes;   
                
                mainAppContext.Tasks.Update(task);
                mainAppContext.SaveChanges();
            
            
            }

            return NotFound(CreateErrorResponse("404","the Task Not Found"));


        }
        [HttpGet("{id}/GEtTask")]
        public async Task<IActionResult> GetTasks(long id,string   status)
        {
            if (string.IsNullOrEmpty(status))
            {

                return BadRequest("Status is requird");

            }
            var task= await mainAppContext.Tasks.Where(t => t.ProjectId == id && t.Status == status).ToListAsync();
            if (task != null) { 
            
            return Ok(CreateSuccessResponse<object>(task));
            }
            return BadRequest(CreateErrorResponse("404", "the project Not Found"));


        }
       
        [HttpPut("{id}")]
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

            if (task.Status.ToLower() == ConstantStatus.Status_To_Do && taskStatusDto.NewStatus.ToLower() != ConstantStatus.Status_IN_Progress)
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
            if (task.Status.ToLower() == ConstantStatus.Status_IN_Progress &&
                taskStatusDto.NewStatus.ToLower() != ConstantStatus.Status_review && taskStatusDto.NewStatus.ToLower() != ConstantStatus.Status_done)
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
            if (task.Status.ToLower() == ConstantStatus.Status_review && taskStatusDto.NewStatus.ToLower() != ConstantStatus.Status_done)
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
            if (task.Status.ToLower() == ConstantStatus.Status_done)
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
            if (taskStatusDto.NewStatus.ToLower() == ConstantStatus.Status_done)
            {
                double precentge = taskService.GetPrecentgeOfTask(task.ProjectId);

                var project = mainAppContext.Projects.Where(p => p.Id == task.ProjectId).FirstOrDefault();
                project.prenctgeTasks=precentge;


                mainAppContext.Projects.Update(project);
               mainAppContext.SaveChanges();

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




    }
}
