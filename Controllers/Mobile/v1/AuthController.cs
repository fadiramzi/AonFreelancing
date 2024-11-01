using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Requests;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Route("api/mobile/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        public AuthController(
            UserManager<User> userManager,
            MainAppContext mainAppContext,
            IConfiguration configuration
            )
        {
            _userManager = userManager;
            _mainAppContext = mainAppContext;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegRequest Req)
        {
            // Enhancement for identifying which user type is
            User u = new User();
            if (Req.UserType == "Freelancer")
            {
                // Register User
                u = new Freelancer
                {
                    Name = Req.Name,
                    UserName = Req.Username,
                    PhoneNumber = Req.PhoneNumber,
                    Skills = "Programming, Net core 8, Communication"
                };
            }
            if (Req.UserType == "Client")
            {
                u = new Models.Client
                {
                    Name = Req.Name,
                    UserName = Req.Username,
                    PhoneNumber = Req.PhoneNumber,
                    CompanyName = Req.CompanyName
                };
            }


            var Result = await _userManager.CreateAsync(u, Req.Password);

            if (!Result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = Result.Errors
                    .Select(e => new Error()
                    {
                        Code = e.Code,
                        Message = e.Description
                    })
                    .ToList()
                });

            }
            // Get created User
            var CreatedUser = await _mainAppContext.Users.OfType<Freelancer>()
                .Where(u => u.UserName == Req.Username)
                .Select(u => new FreelancerResponseDTO()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Username = u.UserName,
                    PhoneNumber = u.PhoneNumber,
                    Skills = u.Skills,
                    UserType = Constants.USER_TYPE_FREELANCER   // // TO-READ (Week 05 Task)we defined constant Freelancer, to avoid code writing error

                })
                .FirstOrDefaultAsync();
            var otp = OTPManager.GenerateOtp();
            // TO-DO(Week 05 Task)
            // This should be enhanced using AppSetting 
            var accountSid = _configuration["Twilio:Sid"];
            var authToken = _configuration["Twilio:Token"];
            TwilioClient.Init(accountSid, authToken);

            var messageOptions = new CreateMessageOptions(
                new PhoneNumber($"whatsapp:{Req.PhoneNumber}")); //To
            messageOptions.From = new PhoneNumber("whatsapp:+14155238886");
            messageOptions.ContentSid = _configuration["Twilio:ContentSid"];
            messageOptions.ContentVariables = "{\"1\":\""+ otp + "\"}";


            var message = MessageResource.Create(messageOptions);

            return Ok(new ApiResponse<FreelancerResponseDTO>()
            {
                IsSuccess = true,
                Errors = [],
                Results = CreatedUser
            });



        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest Req)
        {
            var user = await _userManager.FindByNameAsync(Req.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, Req.Password))
            {
                if (!await _userManager.IsPhoneNumberConfirmedAsync(user))
                {
                    return Unauthorized(new List<Error>() {
                    new Error(){
                        Code = StatusCodes.Status401Unauthorized.ToString(),
                        Message = "Verify Your Account First"
                    }
                });
                }

                // TO-DO(Week 05 - Task)
                // Generate JWT
                var jwt = "";
                // Your Task
           
                return Ok(new ApiResponse<string>
                {
                    IsSuccess = true,
                    Errors = [],
                    Results = jwt

                });

            }

            return Unauthorized(new List<Error>() {
                    new Error(){
                        Code = StatusCodes.Status401Unauthorized.ToString(),
                        Message = "UnAuthorized"
                    }
                });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyReq Req)
        {
            var user = await _userManager.Users.Where(x => x.PhoneNumber == Req.Phone).FirstOrDefaultAsync();
            if (user != null && !await _userManager.IsPhoneNumberConfirmedAsync(user))
            {
                // Get sent OTP to the user
                // Get from DB
                var sentOTP = OTPManager.GenerateOtp();// TO-READ(Week 05 - Task)
                // verify OTP
                if (Req.Otp.Equals(sentOTP))
                {
                    user.PhoneNumberConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    // Delete or disable sent OTP
                    return Ok(new ApiResponse<string>(){
                            IsSuccess = true,
                            Results = "Activated",
                            Errors = []
                    });
                }
            }
            return Unauthorized((new ApiResponse<string>()
            {
                IsSuccess = false,
                Results = null,
                Errors = new List<Error>() {
                    new Error(){
                        Code = StatusCodes.Status401Unauthorized.ToString(),
                        Message = "UnAuthorized"
                    }
                }
            }));
        }

    }
}
