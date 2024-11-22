﻿using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Requests;
using AonFreelancing.Models.Responses;
using AonFreelancing.Models.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Client = AonFreelancing.Models.Client;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Route("api/mobile/v1/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly MainAppContext _mainAppContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly OTPManager _otpManager;
        private readonly JwtService _jwtService;

        public AuthController(
            UserManager<User> userManager,
            MainAppContext mainAppContext,
            RoleManager<ApplicationRole> roleManager,
            OTPManager otpManager,
            JwtService jwtService
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mainAppContext = mainAppContext;
            _otpManager = otpManager;
            _jwtService = jwtService;
        }


        [HttpPost("RegisterByPhone")]

        public async Task <IActionResult> RegisterPhoneNumberAsync([FromBody] RegistByPhoneNumberDTO reg) 
        {
            var user= await _mainAppContext.TemUsers.Where(ph=>ph.phoneNumber==reg.PhoneNumber).FirstOrDefaultAsync();


            if (user != null && !user.PhoneNumberConfirm==false)
            {
                
                return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "Verify Your Account First"));
            }

            user = new TemUser() { phoneNumber=reg.PhoneNumber };

           await _mainAppContext.TemUsers.AddAsync(user);

            string otpCode = _otpManager.GenerateOtp();
            await _mainAppContext.SaveChangesAsync();

            OTP otp = new OTP()
            {
                Code = otpCode,
                PhoneNumber = reg.PhoneNumber,
                CreatedDate = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(1),
            };
           

            //send the otp to the specified phone number
            await _otpManager.SendOTPAsync(otp.Code, reg.PhoneNumber);
            await _mainAppContext.OTPs.AddAsync(otp);
            await _mainAppContext.SaveChangesAsync();
          


            return Ok();


        }

        [HttpPost("sendVerificationCode")]
        public async Task<IActionResult> SendVerificationCodeAsync([FromBody] PhoneNumberReq phoneNumberReq)
        {
            var isUserExists = await _mainAppContext.TempUsers
                .Where(u => u.PhoneNumber == phoneNumberReq.PhoneNumber)
                .FirstOrDefaultAsync();

            if (isUserExists != null)
            {
                return Conflict(CreateErrorResponse(
                    StatusCodes.Status409Conflict.ToString(), "User already exists."));
            }

            var tempUser = new TempUser()
            {
                PhoneNumber = phoneNumberReq.PhoneNumber,
                PhoneNumberConfirmed = false
            };

            _mainAppContext.TempUsers.Add(tempUser);
            await _mainAppContext.SaveChangesAsync();

            var otp = new OTP()
            {
                PhoneNumber = phoneNumberReq.PhoneNumber,
                Code = _otpManager.GenerateOtp(),
                CreatedDate = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(10),
                IsUsed = false,
            };

            _mainAppContext.OTPs.Add(otp);
            await _mainAppContext.SaveChangesAsync();

            await _otpManager.SendOTPAsync(otp.Code, otp.PhoneNumber);

            return Ok(CreateSuccessResponse(otp.Code));
        }

        [HttpPost("verifyPhoneNumber")]
        public async Task<IActionResult> VerifyPhoneNumberAsync([FromBody] VerifyReq verifyReq)
        {
            var tempUser = await _mainAppContext.TempUsers.Where(x => x.PhoneNumber == verifyReq.Phone)
                .FirstOrDefaultAsync();

            var isPhoneNumberConfirmed = await _mainAppContext.TempUsers.AnyAsync(x => x.PhoneNumberConfirmed == true);
            if (tempUser != null && !isPhoneNumberConfirmed)
            {
                var otp = await _mainAppContext.OTPs.Where(o => o.PhoneNumber == verifyReq.Phone)
                    .FirstOrDefaultAsync();

                // verify OTP
                if (otp != null && verifyReq.Otp.Equals(otp.Code) && DateTime.Now < otp.ExpiresAt && otp.IsUsed == false)
                {
                    tempUser.PhoneNumberConfirmed = true;
                    otp.IsUsed = true;
                    await _mainAppContext.SaveChangesAsync();

                    return Ok(CreateSuccessResponse("Activated"));
                }
            }

            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "UnAuthorized"));
        }

        [HttpPost("completeRegistration")]
        public async Task<IActionResult> CompleteRegistrationAsync([FromBody] RegisterRequest registerReq)
        {
            var tempUser =
                await _mainAppContext.TempUsers.FirstOrDefaultAsync(t => t.PhoneNumber == registerReq.PhoneNumber);
            if (tempUser == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "Phone number is invalid."));
            
            User? user = registerReq.UserType switch
            {
                Constants.USER_TYPE_FREELANCER => new Freelancer
                {
                    Name = registerReq.Name,
                    UserName = registerReq.Username,
                    PhoneNumber = tempUser.PhoneNumber,
                    PhoneNumberConfirmed = tempUser.PhoneNumberConfirmed,
                    Skills = registerReq.Skills ?? string.Empty,
                },
                Constants.USER_TYPE_CLIENT => new Client()
                {
                    Name = registerReq.Name,
                    UserName = registerReq.Username,
                    PhoneNumber = tempUser.PhoneNumber,
                    PhoneNumberConfirmed = tempUser.PhoneNumberConfirmed,
                    CompanyName = registerReq.CompanyName ?? string.Empty,
                },
                _ => null
            };

            if (user == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "No such user type exists."));

            var userCreationResult = await _userManager.CreateAsync(user, registerReq.Password);
            if (!userCreationResult.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>()
                {
                    Errors = userCreationResult.Errors
                        .Select(e => new Error()
                        {
                            Code = e.Code,
                            Message = e.Description
                        })
                        .ToList()
                });
            
            var role = new ApplicationRole { Name = registerReq.UserType };
            await _roleManager.CreateAsync(role);
            await _userManager.AddToRoleAsync(user, role.Name);
            _mainAppContext.TempUsers.Remove(tempUser);

            await _mainAppContext.SaveChangesAsync();
            
            return CreatedAtAction(nameof(UsersController.GetProfileByIdAsync), "users", 
                new { id = user.Id }, null);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] AuthRequest req)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == req.PhoneNumber);
            if (user != null && await _userManager.CheckPasswordAsync(user, req.Password))
            {
                if (!await _userManager.IsPhoneNumberConfirmedAsync(user))
                    return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(),
                        "Verify Your Account First"));

                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                var token = _jwtService.GenerateJWT(user, role ?? string.Empty);
                return Ok(CreateSuccessResponse(new LoginResponse()
                {
                    AccessToken = token,
                    UserDetailsDTO = new UserDetailsDTO(user, role ?? string.Empty)
                }));
            }

            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "UnAuthorized"));
        }

        // [HttpPost("forgot-password")]
        // public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordReq req)
        // {
        //     if (string.IsNullOrEmpty(req.PhoneNumber))
        //     {
        //         return BadRequest(new ApiResponse<string>
        //         {
        //             IsSuccess = false,
        //             Results = null,
        //             Errors = new List<Error> {
        //                 new() { Code = StatusCodes.Status400BadRequest.ToString(), Message = "Invalid request." }
        //             }
        //         });
        //     }
        //
        //     var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == req.PhoneNumber);
        //     if (user == null)
        //     {
        //         return Ok(new ApiResponse<string>
        //         {
        //             IsSuccess = true,
        //             Results = "If the phone number is registered, you will receive an OTP.",
        //             Errors = []
        //         });
        //     }
        //
        //     var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //     await _otpManager.SendOTPAsync(user.PhoneNumber ?? string.Empty, token);
        //     
        //     return Ok(new ApiResponse<string>
        //     {
        //         IsSuccess = true,
        //         Results = "If the phone number is registered, you will receive an OTP.",
        //         Errors = []
        //     });
        // }
        //
        // [Authorize(Roles = "Freelancer, Client")]
        // [HttpPost("reset-password")]
        // public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordReq req)
        // {
        //     if (string.IsNullOrEmpty(req.PhoneNumber) || string.IsNullOrEmpty(req.Password))
        //     {
        //         return BadRequest(new ApiResponse<string>
        //         {
        //             IsSuccess = false,
        //             Results = null,
        //             Errors = new List<Error> {
        //                 new() { 
        //                     Code = StatusCodes.Status400BadRequest.ToString(),
        //                     Message = "Invalid request." 
        //                 }
        //             }
        //         });
        //     }
        //
        //     var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == req.PhoneNumber);
        //     if (user == null)
        //     {
        //         return NotFound(new ApiResponse<string>
        //         {
        //             IsSuccess = false,
        //             Results = null,
        //             Errors = new List<Error> {
        //                 new() { 
        //                     Code = StatusCodes.Status404NotFound.ToString(),
        //                     Message = "User not found." 
        //                 }
        //             }
        //         });
        //     }
        //
        //     if (req.Password != req.ConfirmPassword)
        //     {
        //         return BadRequest(new ApiResponse<string>
        //         {
        //             IsSuccess = false,
        //             Results = null,
        //             Errors = new List<Error> {
        //                 new()
        //                 {
        //                     Code = StatusCodes.Status400BadRequest.ToString(), 
        //                     Message = "Passwords do not match."
        //                 }
        //             }
        //         });
        //     }
        //
        //     var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //     var result = await _userManager.ResetPasswordAsync(user, token ,req.Password);
        //
        //     if (result.Succeeded)
        //     {
        //         return Ok(new ApiResponse<string>
        //         {
        //             IsSuccess = true,
        //             Results = "Password reset successfully.",
        //             Errors = []
        //         });
        //     }
        //
        //     return BadRequest(new ApiResponse<string>
        //     {
        //         IsSuccess = false,
        //         Results = null,
        //         Errors = result.Errors.Select(e => new Error
        //         {
        //             Code = e.Code,
        //             Message = e.Description
        //         }).ToList()
        //     });
        // }
    }
}