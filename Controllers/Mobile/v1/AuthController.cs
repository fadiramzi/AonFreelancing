using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Requests;
using AonFreelancing.Models.Responses;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;
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
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<User> userManager,
            MainAppContext mainAppContext,
            RoleManager<ApplicationRole> roleManager,
            OTPManager otpManager,
            JwtService jwtService,
            IConfiguration configuration
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mainAppContext = mainAppContext;
            _otpManager = otpManager;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        [HttpPost("sendVerificationCode")]
        public async Task<IActionResult> SendVerificationCodeAsync([FromBody] PhoneNumberReq phoneNumberReq)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();
            bool isUserExists = await _mainAppContext.Users.AnyAsync(u => u.PhoneNumber == phoneNumberReq.PhoneNumber);
            bool isTempUserExists = await _mainAppContext.TempUsers.AnyAsync(u => u.PhoneNumber == phoneNumberReq.PhoneNumber);
           
            if(isUserExists || isTempUserExists)
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "this phone number is already used"));
            
            var newTempUser = new TempUser()
            {
                PhoneNumber = phoneNumberReq.PhoneNumber,
                PhoneNumberConfirmed = false
            };
            var newOtp = new OTP()
            {
                PhoneNumber = phoneNumberReq.PhoneNumber,
                Code = _otpManager.GenerateOtp(),
                CreatedDate = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Otp:ExpireInMinutes"])),
                IsUsed = false,
            };

            await _mainAppContext.TempUsers.AddAsync(newTempUser);
            await _mainAppContext.OTPs.AddAsync(newOtp);
            await _mainAppContext.SaveChangesAsync();

            await _otpManager.SendOTPAsync(newOtp.Code, newOtp.PhoneNumber);

            if (_configuration["Env"] == Constants.ENV_SIT)
                return Ok(CreateSuccessResponse(newOtp.Code));

            return Ok(CreateSuccessResponse(newOtp.ExpiresAt));
        }

        [HttpPost("verifyPhoneNumber")]
        public async Task<IActionResult> VerifyPhoneNumberAsync([FromBody] VerifyReq verifyReq)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            var storedTempUser = await _mainAppContext.TempUsers.Where(tu => tu.PhoneNumber == verifyReq.Phone)
                .FirstOrDefaultAsync();

            if (storedTempUser != null && !storedTempUser.PhoneNumberConfirmed)
            {
                var otp = await _mainAppContext.OTPs.Where(o => o.PhoneNumber == verifyReq.Phone)
                    .FirstOrDefaultAsync();

                // verify OTP
                if (otp != null && verifyReq.Otp.Equals(otp.Code) && DateTime.Now < otp.ExpiresAt && !otp.IsUsed)
                {
                    storedTempUser.PhoneNumberConfirmed = true;
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
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            var storedTempUser =
                await _mainAppContext.TempUsers.FirstOrDefaultAsync(tu => tu.PhoneNumber == registerReq.PhoneNumber);
            if (storedTempUser == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "Phone number is invalid."));
            
            User? newUser = registerReq.UserType switch
            {
                Constants.USER_TYPE_FREELANCER => new Freelancer
                {
                    Name = registerReq.Name,
                    UserName = registerReq.Username,
                    PhoneNumber = storedTempUser.PhoneNumber,
                    PhoneNumberConfirmed = storedTempUser.PhoneNumberConfirmed,
                    Skills = registerReq.Skills ?? string.Empty,
                },
                Constants.USER_TYPE_CLIENT => new Client()
                {
                    Name = registerReq.Name,
                    UserName = registerReq.Username,
                    PhoneNumber = storedTempUser.PhoneNumber,
                    PhoneNumberConfirmed = storedTempUser.PhoneNumberConfirmed,
                    CompanyName = registerReq.CompanyName ?? string.Empty,
                },
                _ => null
            };

            if (newUser == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "No such user type exists."));

            var userCreationResult = await _userManager.CreateAsync(newUser, registerReq.Password);
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
            await _userManager.AddToRoleAsync(newUser, role.Name);
            _mainAppContext.TempUsers.Remove(storedTempUser);

            await _mainAppContext.SaveChangesAsync();
            
            return CreatedAtAction(nameof(UsersController.GetProfileByIdAsync), "users", 
                new { id = newUser.Id }, null);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] AuthRequest req)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            var storedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == req.PhoneNumber);
            if (storedUser != null && await _userManager.CheckPasswordAsync(storedUser, req.Password))
            {
                if (!await _userManager.IsPhoneNumberConfirmedAsync(storedUser))
                    return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(),
                        "Verify Your Account First"));

                var storedRole = (await _userManager.GetRolesAsync(storedUser)).FirstOrDefault();
                var generatedJWT = _jwtService.GenerateJWT(storedUser, storedRole);
                return Ok(CreateSuccessResponse(new LoginResponse()
                {
                    AccessToken = generatedJWT,
                    UserDetailsDTO = new UserDetailsDTO(storedUser, storedRole ?? string.Empty)
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