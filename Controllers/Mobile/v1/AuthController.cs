using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Requests;
using AonFreelancing.Models.Responses;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML.Messaging;
using Twilio.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Error = AonFreelancing.Models.Error;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Route("api/mobile/v1/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly MainAppContext _mainAppContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly OTPManager _otpManager;
        private readonly JwtService _jwtService;
        public AuthController(
            UserManager<User> userManager,
            MainAppContext mainAppContext,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration configuration,
            OTPManager otpManager,
            JwtService jwtService
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mainAppContext = mainAppContext;
            _configuration = configuration;
            _otpManager = otpManager;
            _jwtService = jwtService;
        }
        [HttpPut("register/resend")]
        public async Task<IActionResult> ResendOtpAsync(RegisterPhoneRequest registerPhoneRequest)
        {
            string otpCode = _otpManager.GenerateOtp();

            TempOTP checkOtp = await _mainAppContext.TempOTPs
                .Where(o => o.PhoneNumber == registerPhoneRequest.PhoneNumber)
                .FirstOrDefaultAsync();

            if (checkOtp == null)
            {
                return NotFound(new { Message = "No OTP entry found for the specified phone number." });
            }

            checkOtp.Code = otpCode;
            checkOtp.CreatedDate = DateTime.Now;
            checkOtp.ExpiresAt = DateTime.Now.AddMinutes(1);

            _mainAppContext.TempOTPs.Update(checkOtp);
            await _mainAppContext.SaveChangesAsync();

            await _otpManager.SendOTPAsync(checkOtp.Code, checkOtp.PhoneNumber);

            return Ok(new { Message = "OTP resent successfully", OTP = otpCode });
        }

        [HttpPost("register/phonenumber")]
        public async Task<IActionResult> RegisterPhoneNumberAsync([FromBody] RegisterPhoneRequest registerPhoneRequest)
        {

            if (await _userManager.Users.Where(u => u.PhoneNumber == registerPhoneRequest.PhoneNumber).FirstOrDefaultAsync() != null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "Phone number is already used by an account"));

            if (await _mainAppContext.UsersTemps.Where(u => u.PhoneNumber == registerPhoneRequest.PhoneNumber && u.IsVerfied == false).FirstOrDefaultAsync() != null)
            {
                if (await _mainAppContext.TempOTPs.Where(u => u.ExpiresAt.HasValue && u.ExpiresAt < DateTime.Now).FirstOrDefaultAsync() != null)
                {
                    return Conflict(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Results = registerPhoneRequest.PhoneNumber,
                        Message = "OTP already sent. Check your messages and verify your account."
                    });
                }
                else
                {
                    await ResendOtpAsync(registerPhoneRequest);
                }
            }

            if (await _mainAppContext.UsersTemps.Where(u => u.PhoneNumber == registerPhoneRequest.PhoneNumber && u.IsVerfied == true).FirstOrDefaultAsync() != null)
                return Conflict(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Results = registerPhoneRequest.PhoneNumber,
                    Message = "Phone number is verified. Complete your registration."
                });

            UsersTemp usersTemp = new UsersTemp
            {
                PhoneNumber = registerPhoneRequest.PhoneNumber
            };

            string otpCode = _otpManager.GenerateOtp();

            TempOTP tempOTP = new TempOTP()
            {
                Code = otpCode,
                PhoneNumber = registerPhoneRequest.PhoneNumber,
                CreatedDate = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(1),
            };

            await _mainAppContext.TempOTPs.AddAsync(tempOTP);
            await _mainAppContext.UsersTemps.AddAsync(usersTemp);
            await _mainAppContext.SaveChangesAsync();

            await _otpManager.SendOTPAsync(tempOTP.Code, tempOTP.PhoneNumber);

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Results = registerPhoneRequest.PhoneNumber,
                Errors = null,
            });


        }

        [HttpPost("register/userInfo")]
        public async Task<IActionResult> RegisterUserInfoAsync([FromBody] RegisterInfoRequest registerInfoRequest)
        {

            User user = new User();
            if (registerInfoRequest.UserType == Constants.USER_TYPE_FREELANCER)
            {
                // Register User
                user = new Freelancer
                {
                    Name = registerInfoRequest.Name,
                    UserName = registerInfoRequest.Username,
                    Email = registerInfoRequest.Email,
                    PhoneNumber = registerInfoRequest.PhoneNumber,
                    Skills = registerInfoRequest.Skills,
                    PhoneNumberConfirmed = true,
                    About = registerInfoRequest.About

                };
            }
            else if (registerInfoRequest.UserType == Constants.USER_TYPE_CLIENT)
            {
                user = new Models.Client
                {
                    Name = registerInfoRequest.Name,
                    UserName = registerInfoRequest.Username,
                    Email = registerInfoRequest.Email,
                    PhoneNumber = registerInfoRequest.PhoneNumber,
                    CompanyName = registerInfoRequest.CompanyName,
                    PhoneNumberConfirmed = true,
                    About = registerInfoRequest.About

                };
            }
            await _userManager.IsPhoneNumberConfirmedAsync(user);
            if (await _userManager.Users.Where(u => u.UserName == registerInfoRequest.Username && u.Email == registerInfoRequest.Email).FirstOrDefaultAsync() != null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "username or email is already used by an account"));

            var userCreationResult = await _userManager.CreateAsync(user, registerInfoRequest.Password);
            if (!userCreationResult.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>()
                {
                    Errors = userCreationResult.Errors.Select(e => new Error()
                    {
                        Code = e.Code,
                        Message = e.Description
                    })
                    .ToList()
                });
            TempOTP setOTP = await _mainAppContext.TempOTPs.Where(x => x.PhoneNumber == registerInfoRequest.PhoneNumber).FirstOrDefaultAsync();
            OTP oTP = new OTP
            {
                PhoneNumber = setOTP.PhoneNumber,
                Code = setOTP.Code,
                CreatedDate = setOTP.CreatedDate,
                IsUsed = setOTP.IsUsed,
                ExpiresAt = setOTP.ExpiresAt

            };
            await _mainAppContext.OTPs.AddAsync(oTP);
            _mainAppContext.TempOTPs.Remove(setOTP);
            await _mainAppContext.SaveChangesAsync();
            
            var role = new ApplicationRole { Name = registerInfoRequest.UserType };
            await _roleManager.CreateAsync(role);
            await _userManager.AddToRoleAsync(user, role.Name);


            var tempuser = await _mainAppContext.UsersTemps.Where(t => t.PhoneNumber == registerInfoRequest.PhoneNumber).FirstOrDefaultAsync();
            _mainAppContext.Remove(tempuser);
            await _mainAppContext.SaveChangesAsync();

            if (registerInfoRequest.UserType == Constants.USER_TYPE_FREELANCER)
            {
                var createdUser = await _mainAppContext.Users.OfType<Freelancer>()
                        .Where(u => u.UserName == registerInfoRequest.Username)
                        .Select(u => new FreelancerResponseDTO()
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Username = u.UserName,
                            PhoneNumber = u.PhoneNumber,
                            Skills = u.Skills,
                            UserType = Constants.USER_TYPE_FREELANCER,
                            IsPhoneNumberVerified = u.PhoneNumberConfirmed,
                            About = u.About,
                            Role = new RoleResponseDTO { Id = role.Id, Name = role.Name }
                        })
                        .FirstOrDefaultAsync();
                return Ok(CreateSuccessResponse(createdUser));
            }
            else if (registerInfoRequest.UserType == Constants.USER_TYPE_CLIENT)
            {
                var createdUser = await _mainAppContext.Users.OfType<Models.Client>()
                          .Where(c => c.UserName == registerInfoRequest.Username)
                          .Select(c => new ClientResponseDTO
                          {
                              Id = c.Id,
                              Name = c.Name,
                              Username = c.UserName,
                              PhoneNumber = c.PhoneNumber,
                              CompanyName = c.CompanyName,
                              UserType = Constants.USER_TYPE_CLIENT,
                              IsPhoneNumberVerified = c.PhoneNumberConfirmed,
                              About = c.About,
                              Role = new RoleResponseDTO { Id = role.Id, Name = role.Name }
                          })
                          .FirstOrDefaultAsync();
                return Ok(CreateSuccessResponse(createdUser));
            }

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] AuthRequest Req)
        {
            var user = await _userManager.FindByNameAsync(Req.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, Req.Password))
            {
                if (!await _userManager.IsPhoneNumberConfirmedAsync(user))
                    return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "Verify Your Account First"));

                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

                if (role == null)
                {
                    return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "User role is missing"));
                }

                var token = _jwtService.GenerateJWT(user, role);
                return Ok(CreateSuccessResponse(new LoginResponse()
                {
                    AccessToken = token,
                    UserDetailsDTO = new UserDetailsDTO(user, role)
                }));
            }

            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "UnAuthorized"));
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyAsync([FromBody] VerifyReq verifyReq)
        {
            var usertemp = await _mainAppContext.UsersTemps.Where(x => x.PhoneNumber == verifyReq.Phone).FirstOrDefaultAsync();
            if (usertemp != null)
            {
                TempOTP? tempOTP = await _mainAppContext.TempOTPs.Where(o => o.PhoneNumber == verifyReq.Phone).FirstOrDefaultAsync();

                if (tempOTP != null && verifyReq.Otp.Equals(tempOTP.Code) && DateTime.Now < tempOTP.ExpiresAt)
                {
                    usertemp.IsVerfied = true;
                    _mainAppContext.UsersTemps.Update(usertemp);

                    tempOTP.IsUsed = true;
                    _mainAppContext.Update(tempOTP);
                    await _mainAppContext.SaveChangesAsync();

                    return Ok(CreateSuccessResponse("Activated"));
                }
            }
            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "UnAuthorized"));
        }

        //[HttpPost("forgotpassword")]
        //public async Task<IActionResult> ForgotPasswordMethod([FromBody] ForgotPasswordReq forgotPasswordRequest)
        //{
        //    var user = await _userManager.Users.Where(u => u.PhoneNumber == forgotPasswordRequest.PhoneNumber).FirstOrDefaultAsync();
        //    if (user != null)
        //    {
        //        string token = await _userManager.GeneratePasswordResetTokenAsync(user);

        //        //send whatsapp  message containing the reset password url
        //        await _otpManager.sendForgotPasswordMessageAsync(token, user.PhoneNumber);
        //    }
        //    // to maintain confidentiality, we always return an OK response even if the user was not found. 
        //    return Ok(CreateSuccessResponse("Check your WhatsApp inbox for password reset token."));
        //}

        //[HttpPost("resetpassword")]
        //public async Task<IActionResult> ResetPasswordMethod([FromBody] ResetPasswordReq resetPasswordReq)
        //{
        //    User? user = await _userManager.Users.Where(u => u.PhoneNumber == resetPasswordReq.PhoneNumber).FirstOrDefaultAsync();
        //    if (user != null)
        //        await _userManager.ResetPasswordAsync(user, resetPasswordReq.Token, resetPasswordReq.Password);

        //    // to maintain confidentiality, we always return an OK response even if the user was not found. 
        //    return Ok(CreateSuccessResponse("Your password have been reset"));
        //}
    }
}