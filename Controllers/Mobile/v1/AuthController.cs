using AonFreelancing.Contexts;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Entities;
using AonFreelancing.Models.Requests;
using AonFreelancing.Models.Responses;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Route("api/mobile/v1/auth")]
    [ApiController]
    public class AuthController(
        UserManager<User> userManager,
        MainAppContext mainAppContext,
        IConfiguration configuration,
        RoleManager<ApplicationRole> roleManager,
        OTPManager otpManager,
        JwtService jwtService)
        : BaseController
    {
        [HttpPost("sendVerificationCode")]
        public async Task<IActionResult> SendVerificationCodeAsync([FromBody] VerificationCodeReq verificationCodeReq)
        {
            var isUserExists = await mainAppContext.TempUsers
                .AnyAsync(u => u.PhoneNumber == verificationCodeReq.PhoneNumber);

            var isPhoneNumberUnique = await mainAppContext.Users
                .AnyAsync(u => u.PhoneNumber == verificationCodeReq.PhoneNumber);
            
            if (isUserExists || isPhoneNumberUnique)
            {
                return Conflict(CreateErrorResponse(
                    StatusCodes.Status409Conflict.ToString(), "User already exists."));
            }

            var tempUser = new TempUser()
            {
                PhoneNumber = verificationCodeReq.PhoneNumber,
                UserType = verificationCodeReq.UserType,
                PhoneNumberConfirmed = false
            };

            mainAppContext.TempUsers.Add(tempUser);
            await mainAppContext.SaveChangesAsync();

            var otpCode = otpManager.GenerateOtp(); 
            if (configuration["Env"] != Constants.ENV_SIT)
            {
                var otp = new OTP()
                {
                    PhoneNumber = verificationCodeReq.PhoneNumber,
                    Code = otpCode,
                    CreatedDate = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddMinutes(10),
                    IsUsed = false,
                };

                mainAppContext.OTPs.Add(otp);
                await mainAppContext.SaveChangesAsync();

                await otpManager.SendOTPAsync(otp.Code, otp.PhoneNumber);
                return Ok(CreateSuccessResponse(otp.ExpiresAt));
            }
            return Ok(CreateSuccessResponse(otpCode));
        }

        [HttpPost("verifyPhoneNumber")]
        public async Task<IActionResult> VerifyPhoneNumberAsync([FromBody] VerifyPhoneNumberReq verifyPhoneNumberReq)
        {
            var tempUser = await mainAppContext.TempUsers.Where(x => x.PhoneNumber == verifyPhoneNumberReq.Phone)
                .FirstOrDefaultAsync();

            var isPhoneNumberConfirmed = await mainAppContext.TempUsers.AnyAsync(x => x.PhoneNumberConfirmed == true);
            if (tempUser != null && !isPhoneNumberConfirmed)
            {
                if (configuration["Env"] == Constants.ENV_SIT && verifyPhoneNumberReq.Otp.Equals("123456"))
                {
                    tempUser.PhoneNumberConfirmed = true;
                    await mainAppContext.SaveChangesAsync();
                    
                    return Ok(CreateSuccessResponse("Activated"));
                }
                
                var otp = await mainAppContext.OTPs.Where(o => o.PhoneNumber == verifyPhoneNumberReq.Phone)
                    .FirstOrDefaultAsync();

                // verify OTP
                if (otp != null && verifyPhoneNumberReq.Otp.Equals(otp.Code) && DateTime.Now < otp.ExpiresAt && otp.IsUsed == false)
                {
                    tempUser.PhoneNumberConfirmed = true;
                    otp.IsUsed = true;
                    await mainAppContext.SaveChangesAsync();

                    return Ok(CreateSuccessResponse("Activated"));
                }
            }

            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "UnAuthorized"));
        }

        [HttpPost("completeRegistration")]
        public async Task<IActionResult> CompleteRegistrationAsync([FromBody] RegisterRequest registerReq)
        {
            var tempUser =
                await mainAppContext.TempUsers.FirstOrDefaultAsync(t => t.PhoneNumber == registerReq.PhoneNumber);
            if (tempUser == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "Phone number is invalid."));
            
            User? user = tempUser.UserType switch
            {
                Constants.USER_TYPE_FREELANCER => new Freelancer
                {
                    Name = registerReq.Name,
                    Email = registerReq.Email,
                    UserName = registerReq.Username,
                    PhoneNumber = tempUser.PhoneNumber,
                    PhoneNumberConfirmed = tempUser.PhoneNumberConfirmed,
                    Skills = registerReq.Skills ?? string.Empty,
                },
                Constants.USER_TYPE_CLIENT => new Client()
                {
                    Name = registerReq.Name,
                    UserName = registerReq.Username,
                    Email = registerReq.Email,
                    PhoneNumber = tempUser.PhoneNumber,
                    PhoneNumberConfirmed = tempUser.PhoneNumberConfirmed,
                    CompanyName = registerReq.CompanyName ?? string.Empty,
                },
                _ => null
            };

            if (user == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "No such user type exists."));

            var userCreationResult = await userManager.CreateAsync(user, registerReq.Password);
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
            
            var role = new ApplicationRole { Name = tempUser.UserType };
            await roleManager.CreateAsync(role);
            await userManager.AddToRoleAsync(user, role.Name);
            mainAppContext.TempUsers.Remove(tempUser);

            await mainAppContext.SaveChangesAsync();
            
            return CreatedAtAction(nameof(UsersController.GetProfileByIdAsync), "users", 
                new { id = user.Id }, null);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] AuthRequest req)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == req.PhoneNumber);
            if (user != null && await userManager.CheckPasswordAsync(user, req.Password))
            {
                if (!await userManager.IsPhoneNumberConfirmedAsync(user))
                    return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(),
                        "Verify Your Account First"));

                var role = (await userManager.GetRolesAsync(user)).FirstOrDefault();
                var token = jwtService.GenerateJwt(user, role ?? string.Empty);
                return Ok(CreateSuccessResponse(new LoginResponse()
                {
                    AccessToken = token,
                    UserDetails = new UserDetailsDTO(user, role ?? string.Empty)
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