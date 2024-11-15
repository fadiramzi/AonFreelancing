﻿using AonFreelancing.Contexts;
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
using Twilio.Rest.Serverless.V1.Service.Environment;

namespace AonFreelancing.Controllers.Mobile.v1
{
    // [Authorize]
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
                                                                .Select(f => new FreelancerResponseDTO
                                                                {
                                                                    Id = f.Id,
                                                                    Name = f.Name,
                                                                    Email = f.Email,
                                                                    PhoneNumber = f.PhoneNumber,
                                                                    UserType = Constants.USER_TYPE_FREELANCER,
                                                                    Skills = f.Skills,
                                                                    About = f.About
                                                                }).FirstOrDefaultAsync();
            if (freelancerResponseDTO != null)
                return Ok(CreateSuccessResponse(freelancerResponseDTO));

            
            var clientResponseDTO = await _mainAppContext.Users.OfType<Client>()
                                                                .Where(c => c.Id == id)
                                                                .Include(c => c.Projects)
                                                                .Select(c => new ClientResponseDTO
                                                                {
                                                                    Id = c.Id,
                                                                    Name = c.Name,
                                                                    Email = c.Email,
                                                                    PhoneNumber = c.PhoneNumber,
                                                                    UserType = Constants.USER_TYPE_CLIENT,
                                                                    CompanyName = c.CompanyName,
                                                                    Projects = c.Projects.Where(p => p.Status == "Closed")
                                                                                        .Select(p => new ProjectHistoryOutDTO
                                                                                        {
                                                                                            Id = p.Id,
                                                                                            Title = p.Title,
                                                                                            CreatedAt = p.CreatedAt,
                                                                                            EndDate = p.ProjectHistory.EndDate,
                                                                                            Description = p.Description
                                                                                        }).ToList()
                                                                }).FirstOrDefaultAsync();
            if (clientResponseDTO != null)
                return Ok(CreateSuccessResponse(clientResponseDTO));

            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "User not found"));
        }
    }
}