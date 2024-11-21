using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Requests;
using AonFreelancing.Models.Responses;
using AonFreelancing.Models.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography.Xml;



namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/projects")]
    [ApiController]

    public class ProjectsController(MainAppContext mainAppContext, UserManager<User> userManager,FileService fileService) : BaseController


    {
     
       

        [Authorize(Roles = "CLIENT")]
        [HttpPost]
        public async Task<IActionResult> PostProjectAsync([FromBody] ProjectInputDto projectInputDto)
        {
            if (!ModelState.IsValid)
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

            if (projectInputDto.ImageName != null)
            {
                string fileName = await fileService.SaveAsync(projectInputDto.ImageName);
                project.ImageName = fileName;
            }

            await mainAppContext.Projects.AddAsync(project);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Project added."));
        }

        [Authorize(Roles = "CLIENT")]
        [HttpGet("clientFeed")]
        public async Task<IActionResult> GetClientFeedAsync(
            [FromQuery] List<string>? qualificationNames, [FromQuery] int page = 0,
            [FromQuery] int pageSize = 8, [FromQuery] string? qur = default
        )
        {
            var imagesBaseUrl = $"{Request.Scheme}://{Request.Host}/images";
            var trimmedQuery = qur?.ToLower().Replace(" ", "").Trim();
            List<ProjectOutDTO>? projects;

            var query = mainAppContext.Projects.AsQueryable();

            var count = await query.CountAsync();

            if (!string.IsNullOrEmpty(trimmedQuery))
            {
                query = query
                    .Where(p => p.Title.ToLower().Contains(trimmedQuery));
            }
            if (qualificationNames != null && qualificationNames.Count > 0)
            {
                query = query
                    .Where(p => qualificationNames.Contains(p.QualificationName));
            }

            // ORder by LAtest created
            projects = await query.OrderByDescending(p => p.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectOutDTO(p,imagesBaseUrl)
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

            return Ok(CreateSuccessResponse(new
            {
                Total = count,
                Items = projects
            }));
        }



        [Authorize(Roles = "FREELANCER")]
        [HttpPost("{id}/bids")]


        public async Task<IActionResult> GetProjectToBids([FromBody] BidsRequset bidrq, long id)
        {

            var user = await userManager.GetUserAsync(User);

            var project = await mainAppContext.Projects.Where(p => p.Id == id).FirstOrDefaultAsync();

            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            var lastproposed = mainAppContext.Bids.Where(b => b.ProjectId == id).OrderByDescending(b => b.submittedAt).FirstOrDefault();
            if (user != null)
            {
                if (!(rol == Constants.USER_TYPE_FREELANCER))
                {
                    return Unauthorized(CreateErrorResponse(StatusCodes.Status403Forbidden.ToString(), "you are not Freelancer"));

                }
                if (bidrq.proposed < 0)
                {
                    return BadRequest("you must not  put price in negative");

                }


                if (bidrq.proposed < project.Budget && bidrq.proposed < lastproposed.proposed_Price && user != null)
                {

                    var bid = new Bid()
                    {

                        FreelancerId = user.Id,
                        proposed_Price = bidrq.proposed,
                        Nots = bidrq.Nots,
                        ProjectId = project.Id,
                        submittedAt = DateTime.Now,


                    };

                    return Ok(CreateSuccessResponse<object>(bid));
                }
                else
                {

                    return BadRequest("you must put price less");
                }
            }

            return NotFound("the User NOt Found");
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetProject(long id)
        {
            string imageUrL = $"{Request.Scheme}://{Request.Host}/images";
            var project = await mainAppContext.Projects.Where(p => p.Id == id).Include(p => p.Bids.OrderBy(b => b.proposed_Price)).Select(p=>new ProjectOutDTO(p,imageUrL)).FirstOrDefaultAsync();

            if (project == null)
            {
                return NotFound(CreateErrorResponse("400","Not Found"));



            }



            return Ok(CreateSuccessResponse<object>(project));
            
              
          

        }

        [Authorize(Roles = "CLIENT")]
        [HttpPatch("{pid}/bids/{bid}/approve")]
        public async Task<IActionResult> AprroverBids(long Pid, long bid)
        {
            var project = await mainAppContext.Projects.FindAsync(Pid);


            if (project == null || project.Status != "available")
            {

                return BadRequest(CreateErrorResponse("400", "the project is not available"));
            }
            var bidd = await mainAppContext.Bids.FindAsync(bid);
            if (bidd.status == "pending")
            {
                project.Status = "closed";
                bidd.status = "approved";
                bidd.ApprovedAt = DateTime.Now;

                mainAppContext.Projects.Update(project);
                mainAppContext.SaveChanges();

                mainAppContext.Bids.Update(bidd);
                mainAppContext.SaveChanges();

            }
            return NotFound("The Project Not Found");

        }


        [HttpPost("{id}/tasks")]

        public async Task<IActionResult> ProjectTasks([FromBody] TaskRq Rq, int id)
        {
            var project = await mainAppContext.Projects.FindAsync(id);

            if (project != null)
            {
                var task = new EntityTask()
                {
                    ProjectId = project.Id,
                    Name = Rq.Name,
                    DedlineAt = Rq.DeadLine,

                };


                await mainAppContext.Tasks.AddAsync(task);
                await mainAppContext.SaveChangesAsync();
                return Ok();
            }
            return NotFound(CreateErrorResponse("400","the project Task Not Found"));


        }
        [Authorize(Roles = "CLIENT")]
        [HttpPatch("/task{id}")]
        public async Task<IActionResult> UpdateTaskStatus([FromBody] TaskStatusRq Rq, int id)
        {
            var Task = await mainAppContext.Tasks.FindAsync(id);
            if (Task != null)
            {
                Task.status = Rq.newStatus;

                mainAppContext.Tasks.Update(Task);
                await mainAppContext.SaveChangesAsync();
            }
            return NotFound(CreateErrorResponse("400", "the project Not Found"));

        }

        //[Authorize(Roles = "CLIENT")]
        //[HttpPost("upload-image")]
        //public async Task<IActionResult> UploadImage(IFormFile file, int id)
        //{

        //    var user = await mainAppContext.Users.FindAsync(id);
        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest("No file uploaded.");
        //    }

        //    if (file.Length > 10 * 1024 * 1024)
        //    {
        //        return BadRequest("File size is too large.");
        //    }
        //    var fileName = Path.GetFileNameWithoutExtension(file.FileName);
        //    var extension = Path.GetExtension(file.FileName).ToLower();
        //    var newFileName = $"{fileName}_{Guid.NewGuid()}{extension}";
        //    var filePath = Path.Combine(_uploadFolder, newFileName);
        //    // Validate file type (example: allow only image files)
        //    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

        //    if (!allowedExtensions.Contains(extension))
        //    {
        //        return BadRequest("Invalid file type.");
        //    }

        //    // Convert the image file to byte array
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        await file.CopyToAsync(memoryStream);
        //        var imageData = memoryStream.ToArray();

        //        // Create a new ImageModel instance
        //        var image = new Image
        //        {
        //            Name = file.FileName,
        //            ImageData = imageData,
        //            Contenttype = file.ContentType,
        //            UserId = user.Id

        //        };

        //        // Save the image to the database
        //        mainAppContext.Images.Add(image);
        //        await mainAppContext.SaveChangesAsync();


        //        return Ok(new { filePath = $"/uploads/{newFileName}" });
        //    }


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

