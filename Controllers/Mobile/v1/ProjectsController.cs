
using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/projects")]
    [ApiController]
    public class ProjectsController(MainAppContext mainAppContext, UserManager<User> userManager) : BaseController
    {
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
                PriceType = "Fixed",
                CreatedAt = DateTime.Now,
            };

            await mainAppContext.Projects.AddAsync(project);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Project added."));
        }

        [Authorize(Roles = "CLIENT")]
        [HttpGet("clientFeed")]
        public async Task<IActionResult> GetClientFeedAsync(
            [FromQuery] List<string>? qualificationNames, [FromQuery] int page = 1,
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
            .Skip((page-1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectOutDTO
            {
                Id= p.Id,
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
        public async Task<IActionResult> SubmitBidAsync(long id, [FromBody] BidInputDto bidDto)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var project = await mainAppContext.Projects.FindAsync(id);
            if (project == null)
                return NotFound(CreateErrorResponse("404", "Project not found."));

            if (project.Status==Constants.PROJECT_STATUS_CLOSED)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                   "project is closed "));

            var user = await userManager.GetUserAsync(User);
            //if (user == null || !User.IsInRole("FREELANCER"))
            //    return Forbid();

            var lastBid = await mainAppContext.Bids
                .Where(b => b.ProjectId == id)
                .OrderByDescending(b => b.SubmittedAt)
                .FirstOrDefaultAsync();

            if (bidDto.ProposedPrice <= 0 ||
                (lastBid != null && bidDto.ProposedPrice > lastBid.ProposedPrice) ||
                (lastBid == null && bidDto.ProposedPrice > project.Budget))
            {
                return BadRequest(CreateErrorResponse("400", "Invalid proposed price. The proposed price must be positive and lower than the last bid or project budget."));
            }
           
            var bid = new Bid
            {
                ProjectId = id,
                FreelancerId = user.Id,
                ProposedPrice = bidDto.ProposedPrice,
                Notes = bidDto.Notes,
                Status = Constants.BIDS_STATUS_PENDING, 
                SubmittedAt = DateTime.Now
            };

            await mainAppContext.Bids.AddAsync(bid);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Bid submitted successfully."));
        }


        [Authorize(Roles = "CLIENT")]
        [HttpPut("{pid}/bids/{bid}/approve")]
        public async Task<IActionResult> ApproveBidAsync(long pid, long bid)
        {
            var project = await mainAppContext.Projects.FindAsync(pid);
            if (project == null || project.Status != Constants.PROJECT_STATUS_AVAILABLE)
                return BadRequest(CreateErrorResponse("400", $"Project status is '{project?.Status}', but must be 'available'."));

            var bidID = await mainAppContext.Bids.FirstOrDefaultAsync(b => b.Id == bid);
            if (bidID == null || bidID.ProjectId != pid || bidID.Status == Constants.BIDS_STATUS_APPROVED)
                return BadRequest(CreateErrorResponse("400", "Bid not found or already approved."));

            bidID.Status = Constants.BIDS_STATUS_APPROVED;
            bidID.ApprovedAt = DateTime.Now;

            project.Status = Constants.PROJECT_STATUS_CLOSED;
            project.FreelancerId= bidID.FreelancerId;
            project.StartDate= DateTime.Now;
            project.EndDate = DateTime.Now.AddDays(project.Duration);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Bid approved successfully."));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectDetailsAsync(long id)
        {
            var project = await mainAppContext.Projects
                .Where(p => p.Id == id)
                .Include(p => p.Bids)
                .ThenInclude(b => b.Freelancer)
                .FirstOrDefaultAsync();

            if (project == null)
                return NotFound(CreateErrorResponse("404", "Project not found."));

            var orderedBids = project.Bids
                .OrderByDescending(b => b.ProposedPrice)
                .Select(b => new BidOutDto { 
                Id = b.Id,
                FreelancerId = b.FreelancerId,
                Freelancer = new FreelancerShortOutDTO { 
                    Id = b.FreelancerId,
                    Name = b.Freelancer.Name
                },
                ProposedPrice = b.ProposedPrice,
                Notes = b.Notes,
                Status = b.Status,
                SubmittedAt = b.SubmittedAt,
                ApprovedAt = b.ApprovedAt
                } );


          
            return Ok(CreateSuccessResponse(new
            {
                project.Id,
                project.Title,
                project.Status,
                project.Budget,
                project.Duration,
                project.Description, 
                Bids = orderedBids
            }));
        }


        [Authorize(Roles = "CLIENT")]
        [HttpPost("{id}/tasks")]
        public async Task<IActionResult> CreateTaskAsync(long id, [FromBody] TaskInputDto taskDto)
        {
            var project = await mainAppContext.Projects.FindAsync(id);
            if (project == null || project.Status != Constants.PROJECT_STATUS_CLOSED)
                return BadRequest(CreateErrorResponse("400", "Project not found or not closed."));

            var task = new TaskEntity
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


        [Authorize(Roles = "CLIENT")]
        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadProjectImage(long id, IFormFile file)
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

        [HttpGet("{id}/tasks")]
        public async Task<IActionResult> GetProjectTasksAsync([FromQuery] string? status ,int id )
        {
           List<TaskOutDTO> tasks = new List<TaskOutDTO>();
            if (status == null) { 
             tasks= await mainAppContext.Tasks.Where(t=>t.ProjectId == id&&t.IsDeleted == false)
                    .Select(t => new TaskOutDTO
                    {
                        Name = t.Name,
                    })
                    .ToListAsync();

            }
            if (status != null) { 
                
                 tasks = await mainAppContext.Tasks.Where(t => t.ProjectId == id && t.IsDeleted == false && t.Status==status)
                    .Select(t=>new TaskOutDTO{
                    Name=t.Name,
                    })
                    .ToListAsync();
               
            }
            if (tasks.Any())
            {
                return Ok(CreateSuccessResponse(tasks));
            }
            else
            {
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    $"project has no {status} tasks"));
            }

        }
        [Authorize(Roles = "FREELANCER")]
        [HttpGet("freelancerFeed")]
        public async Task<IActionResult> GetFreelancerFeedAsync([FromQuery] FreeelnacerFeedInputDTO freeelnacerFeedInputDTO,
           [FromQuery] List<string>? Specialization, [FromQuery] int page = 1,
           [FromQuery] int pageSize = 8
       )
        {

            int period = 0;
            List<ProjectOutDTO>? projects;

            var query = mainAppContext.Projects.AsQueryable();

            var count = await query.CountAsync();

           
            if (Specialization != null && Specialization.Count > 0)
            {
                query = query
                    .Where(p => Specialization.Contains(p.QualificationName));
            }
            if (freeelnacerFeedInputDTO.DurationType == Constants.DURATION_TYPE_MONTH)
            {
                 period = 30;
            }
            if (freeelnacerFeedInputDTO.DurationType == Constants.DURATION_TYPE_YEAR)
            {
                 period = 365;
            }
            if (query != null)
            {
                query = query.Where(q => q.Duration <= (freeelnacerFeedInputDTO.DurationPeriod * period)
               && q.Budget >= freeelnacerFeedInputDTO.MinPrice && q.Budget <= freeelnacerFeedInputDTO.MaxPrice);
            }
            projects = await query.OrderByDescending(p => p.CreatedAt)
            .Skip((page-1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectOutDTO
            {
                Id = p.Id,
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

            return Ok(CreateSuccessResponse(new
            {
                Total = count,
                Items = projects
            }));
        }

        //[HttpGet("{id}")]
        //public IActionResult GetProject(int id)
        //{
        //    var project = _mainAppContext.Projects
        //        .Include(p => p.Client)
        //        .FirstOrDefault(p => p.Id == id);

        //    return Ok(CreateSuccessResponse(project));

        //}
    }
}