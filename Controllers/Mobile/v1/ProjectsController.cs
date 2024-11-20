using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
<<<<<<< HEAD
=======
using System.IO;
>>>>>>> 7a1bf9d3c70dc397651dcc412af417235d1c26a5

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/projects")]
    [ApiController]
    public class ProjectsController(MainAppContext mainAppContext, UserManager<User> userManager) : BaseController
    {
<<<<<<< HEAD
        private readonly MainAppContext _mainAppContext;
        private readonly UserManager<User> _userManager;
        public ProjectsController(
            MainAppContext mainAppContext,
            UserManager<User> userManager
            )
        {
            _mainAppContext = mainAppContext;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectInputDTO project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<ProjectOutDTO>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error> { new Error { Code = "400", Message = "Invalid project data." } },
                    Message = "Validation failed"
                });
            }

            try
            {
                Project p = new Project
                {
                    Title = project.Title,
                    Description = project.Description,
                    ClientId = project.ClientId,
                    PriceType = project.PriceType,
                    Duration = project.Duration,
                    QualificationName = project.QualificationName,
                    Budget = project.Budget,
                    Status = "Available"
                };

                await _mainAppContext.Projects.AddAsync(p);
                await _mainAppContext.SaveChangesAsync();

                var projectOutDTO = new ProjectOutDTO
                {
                    Title = p.Title,
                    Description = p.Description,
                    CreatedAt = DateTime.UtcNow,
                    Status = p.Status,
                    Budget = p.Budget,
                    Duration = p.Duration,
                    PriceType = p.PriceType,
                    QualificationName = p.QualificationName
                };

                return Ok(new ApiResponse<ProjectOutDTO>
                {
                    IsSuccess = true,
                    Results = projectOutDTO,
                    Errors = null,
                    Message = "Project created successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ProjectOutDTO>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error> { new Error { Code = "500", Message = ex.Message } },
                    Message = "An error occurred while creating the project."
                });
            }
=======
        [Authorize(Roles = "CLIENT")]
        [HttpPost]
        public async Task<IActionResult> PostProjectAsync([FromBody] ProjectInputDto projectInputDto)
        {
            if(!ModelState.IsValid)
            {
                
                return base.CustomBadRequest();
            }
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Unable to load user."));

            var userId = user.Id;
            var project = new Project
            {
                ClientId = userId,
                Title = projectInputDto.Title,
                Description = projectInputDto.Description,
                QualificationName = projectInputDto.QualificationName,
                Duration = projectInputDto.Duration,
                Budget = projectInputDto.Budget,
                PriceType = projectInputDto.PriceType,
                CreatedAt = DateTime.Now,
            };

            await mainAppContext.Projects.AddAsync(project);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Project added."));
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2
        }

        [Authorize(Roles = "CLIENT")]
        [HttpGet("clientFeed")]
        public async Task<IActionResult> GetClientFeedAsync(
            [FromQuery] List<string>? qualificationNames, [FromQuery] int page = 0,
            [FromQuery] int pageSize = 8, [FromQuery] string? qur = default
        )
        {
            var trimmedQuery = qur?.ToLower().Replace(" ", "").Trim();
            List<ProjectOutDTO>? projects;

            var query = mainAppContext.Projects.AsQueryable();

            var count = await query.CountAsync();

            if(!string.IsNullOrEmpty(trimmedQuery))
            {
                query = query
                    .Where(p=>p.Title.ToLower().Contains(trimmedQuery));
            }
            if(qualificationNames != null && qualificationNames.Count >0)
            {
                query = query
                    .Where(p => qualificationNames.Contains(p.QualificationName));
            }

            projects = await query.OrderByDescending(p => p.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectOutDTO
            {

                Title = p.Title,
                Description = p.Description,
                Status = p.Status,
                Budget = p.Budget,
                Duration = p.Duration,
                PriceType = p.PriceType,
                Qualifications = p.QualificationName,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CreatedAt = p.CreatedAt,
                CreationTime = StringOperations.GetTimeAgo(p.CreatedAt)
            })
            .ToListAsync();
           
            return Ok(CreateSuccessResponse(new { 
                Total=count,
                Items=projects
            }));
        }


        [Authorize(Roles = "FREELANCER")]
        [HttpPost("{id}/bids")]
        public async Task<IActionResult> SubmitBidAsync(int id, [FromBody] BidInputDto bidDto)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var project = await mainAppContext.Projects.FindAsync(id);
            if (project == null)
                return NotFound(CreateErrorResponse("404", "Project not found."));

            var user = await userManager.GetUserAsync(User);
            if (user == null || !User.IsInRole("FREELANCER"))
                return Forbid();

            var lastBid = await mainAppContext.Bids
                .Where(b => b.ProjectId == id)
                .OrderByDescending(b => b.SubmittedAt)
                .FirstOrDefaultAsync();

            if (bidDto.ProposedPrice <= 0 ||
                (lastBid != null && bidDto.ProposedPrice >= lastBid.ProposedPrice) ||
                (lastBid == null && bidDto.ProposedPrice >= project.Budget))
            {
                return BadRequest(CreateErrorResponse("400", "Invalid proposed price. The proposed price must be positive and lower than the last bid or project budget."));
            }

            var bid = new Bid
            {
                ProjectId = id,
                FreelancerId = user.Id,
                ProposedPrice = bidDto.ProposedPrice,
                Notes = bidDto.Notes,
                Status = "pending", 
                SubmittedAt = DateTime.Now
            };

            await mainAppContext.Bids.AddAsync(bid);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Bid submitted successfully."));
        }


        [Authorize(Roles = "CLIENT")]
        [HttpPut("{pid}/bids/{bid}/approve")]
        public async Task<IActionResult> ApproveBidAsync(int pid, int bid)
        {
            var project = await mainAppContext.Projects.FindAsync(pid);
            if (project == null || project.Status != "Available")
                return BadRequest(CreateErrorResponse("400", $"Project status is '{project?.Status}', but must be 'available'."));

            var bidID = await mainAppContext.Bids.FirstOrDefaultAsync(b => b.Id == bid);
            if (bidID == null || bidID.ProjectId != pid || bidID.Status == "approved")
                return BadRequest(CreateErrorResponse("400", "Bid not found or already approved."));

            bidID.Status = "approved";
            bidID.ApprovedAt = DateTime.Now;

            project.Status = "Closed";

            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Bid approved successfully."));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectDetailsAsync(int id)
        {
            var project = await mainAppContext.Projects
                .Where(p => p.Id == id)
                .Include(p => p.Bids)
                .FirstOrDefaultAsync();

            if (project == null)
                return NotFound(CreateErrorResponse("404", "Project not found."));

            var orderedBids = project.Bids
                .OrderBy(b => b.ProposedPrice)
                .Select(b => new
                {
                    b.Id,
                    b.FreelancerId,
                    b.ProposedPrice,
                    b.Notes,
                    b.Status,
                    b.SubmittedAt,
                    b.ApprovedAt
                });

            return Ok(new
            {
                project.Id,
                project.Title,
                project.Status,
                project.Budget,
                Bids = orderedBids
            });
        }


        [Authorize(Roles = "CLIENT")]
        [HttpPost("{id}/tasks")]
        public async Task<IActionResult> CreateTaskAsync(int id, [FromBody] TaskInputDto taskDto)
        {
            var project = await mainAppContext.Projects.FindAsync(id);
            if (project == null || project.Status != "Closed")
                return BadRequest(CreateErrorResponse("400", "Project not found or not closed."));

            var task = new Tasks
            {
                ProjectId = id,
                Name = taskDto.Name,
                DeadlineAt = taskDto.DeadlineAt,
                Notes = taskDto.Notes
            };

            await mainAppContext.Tasks.AddAsync(task);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Task created successfully."));
        }


        [Authorize(Roles = "CLIENT, FREELANCER")]
        [HttpPut("tasks/{id}")]
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


        [Authorize(Roles = "CLIENT")]
        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadProjectImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error> { new Error { Code = "400", Message = "No file uploaded." } }
                });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error> { new Error { Code = "400", Message = "Invalid file type. Only image files are allowed." } }
                });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error> { new Error { Code = "400", Message = "File size exceeds the 5 MB limit." } }
                });
            }

            // Define the file path to save the image (e.g., in wwwroot/images)
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate a unique file name
            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadPath, fileName);

            // Save the image to the server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save the file metadata to the database 
            var project = await mainAppContext.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error> { new Error { Code = "404", Message = "Project not found." } }
                });
            }

            // Save the image path or filename to the project model
            project.ImagePath = $"/images/{fileName}";
            await mainAppContext.SaveChangesAsync();

            // Return a success response with the image URL or file path
            return Ok(new ApiResponse<string>
            {
                IsSuccess = true,
                Results = $"/images/{fileName}",
                Errors = null
            });
        }

        //[HttpGet("{id}")]
        //public IActionResult GetProject(int id)
        //{
        //    var project = _mainAppContext.Projects
        //        .Include(p => p.Client)
        //        .FirstOrDefault(p => p.Id == id);

        //    return Ok(CreateSuccessResponse(project));

        //}
<<<<<<< HEAD

        [Authorize]
        [HttpGet("feed")]
        public async Task<IActionResult> GetProjectsFeed(
    int pageSize = 10,
    int pageNumber = 1,
    string search_query = "",
    string qual = "")
        {
            var query = _mainAppContext.Projects.AsQueryable();

            query = query.Where(p => p.Status == "Available");

            if (!string.IsNullOrEmpty(search_query))
            {
                query = query.Where(p => p.Title.Contains(search_query) || p.Description.Contains(search_query));
            }

            if (!string.IsNullOrEmpty(qual))
            {
                var qualifications = qual.Split(',');
                query = query.Where(p => qualifications.Contains(p.QualificationName));
            }

            var totalProjects = await query.CountAsync();

            var projects = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var projectDtos = projects.Select(p => new ProjectOutDTO
            {
                Title = p.Title,
                Description = p.Description,
                Status = p.Status,
                Budget = p.Budget,
                Duration = p.Duration,
                QualificationName = p.QualificationName,
                CreatedAt = DateTime.UtcNow,
                CreationTimeAgo = GetTimeAgo(p.CreatedAt)
            }).ToList();

            var response = new ApiResponse<IEnumerable<ProjectOutDTO>>
            {
                IsSuccess = true,
                Results = projectDtos,
                Message = "Projects feed loaded successfully",
                Errors = null
            };

            Response.Headers.Add("X-Total-Count", totalProjects.ToString());
            Response.Headers.Add("X-Total-Pages", Math.Ceiling(totalProjects / (double)pageSize).ToString());

            return Ok(response);
        }

        private string GetTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.UtcNow - createdAt;
            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute(s) ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour(s) ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} day(s) ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} month(s) ago";
            return $"{(int)(timeSpan.TotalDays / 365)} year(s) ago";
        }

=======
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2
    }
}