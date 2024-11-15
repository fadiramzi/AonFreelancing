using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/users")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly MainAppContext _mainAppContext;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public UsersController(MainAppContext mainAppContext, RoleManager<ApplicationRole> roleManager)
        {
            _mainAppContext = mainAppContext;
            _roleManager = roleManager;
        }

        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetProfileById([FromRoute] long id)
        {
            var freelancerResponseDTO = await _mainAppContext.Users.OfType<Freelancer>()
                                                                .Where(f => f.Id == id)
                                                                .Select(f => new FreelancerProfileDTO
                                                                {
                                                                    Id = f.Id,
                                                                    Name = f.Name,
                                                                    Skills = f.Skills,
                                                                    PhoneNumber = f.PhoneNumber,
                                                                    Email = f.Email,
                                                                    About = f.About,
                                                                    ProjectHistory = f.Projects.Select(p => new ProjectHistoryDTO
                                                                    {
                                                                        Id = p.Id,
                                                                        Title = p.Title,
                                                                        StartDate = p.StartDate,
                                                                        EndDate = p.EndDate,
                                                                        Description = p.Description
                                                                    }).ToList()
                                                                }).FirstOrDefaultAsync();
            if (freelancerResponseDTO != null)
                return Ok(CreateSuccessResponse(freelancerResponseDTO));


            var clientResponseDTO = await _mainAppContext.Users.OfType<Client>()
                                                                .Where(c => c.Id == id)
                                                                .Include(c => c.Projects)
                                                                .Select(c => new ClientProfileDTO
                                                                {
                                                                    Id = c.Id,
                                                                    Name = c.Name,
                                                                    CompanyName = c.CompanyName,
                                                                    PhoneNumber = c.PhoneNumber,
                                                                    Email = c.Email,
                                                                    About = c.About,
                                                                    ProjectHistory = c.Projects.Select(p => new ProjectHistoryDTO
                                                                    {
                                                                        Id = p.Id,
                                                                        Title = p.Title,
                                                                        StartDate = p.StartDate,
                                                                        EndDate = p.EndDate,
                                                                        Description = p.Description
                                                                    }).ToList()
                                                                }).FirstOrDefaultAsync();

            if (clientResponseDTO != null)
                return Ok(CreateSuccessResponse(clientResponseDTO));

            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "User not found"));
        }

    }
}
