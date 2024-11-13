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

        [HttpPost("sendVerificationCode")]
        public async Task<IActionResult> SendVerificationCodeAsync([FromBody] PhoneNumberReq phoneNumberReq)
        {
            try
            {
                var isUserExists = await _mainAppContext.TempUsers
                    .Where(u => u.PhoneNumber == phoneNumberReq.PhoneNumber
                                || u.PhoneNumber == phoneNumberReq.PhoneNumber)
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
                    ExpiresAt = DateTime.Now.AddMinutes(1),
                    IsUsed = false,
                };

                _mainAppContext.OTPs.Add(otp);
                await _mainAppContext.SaveChangesAsync();

                await _otpManager.SendOTPAsync(otp.Code, otp.PhoneNumber);

                return Ok(CreateSuccessResponse(otp.Code));
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
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
                if (otp != null && verifyReq.Otp.Equals(otp.Code) && DateTime.Now < otp.ExpiresAt)
                {
                    tempUser.PhoneNumberConfirmed = true;
                    otp.IsUsed = true;
                    await _mainAppContext.SaveChangesAsync();

                    return Ok(CreateSuccessResponse("Activated"));
                }
            }

            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "UnAuthorized"));
        }

        //ToDo: Next modify registration endpoint
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest registerReq)
        {
            var tempUser =
                await _mainAppContext.TempUsers.FirstOrDefaultAsync(x => x.PhoneNumber == registerReq.PhoneNumber);
            if (tempUser == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "No such user type exists."));


            User? user = registerReq.UserType switch
            {
                Constants.USER_TYPE_FREELANCER => new Freelancer
                {
                    Name = registerReq.Name,
                    UserName = registerReq.Username,
                    PhoneNumber = tempUser?.PhoneNumber,
                    PhoneNumberConfirmed = (bool)tempUser?.PhoneNumberConfirmed,
                    Skills = registerReq.Skills ?? string.Empty
                },
                Constants.USER_TYPE_CLIENT => new Client()
                {
                    Name = registerReq.Name,
                    UserName = registerReq.Username,
                    PhoneNumber = tempUser?.PhoneNumber,
                    PhoneNumberConfirmed = (bool)tempUser?.PhoneNumberConfirmed,
                    CompanyName = registerReq.CompanyName ?? string.Empty
                },
                _ => null
            };

            if (user == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "No such user type exists."));
            
            // _mainAppContext.TempUsers.Remove(tempUser);
            // await _mainAppContext.SaveChangesAsync();

            if (await _userManager.Users
                    .Where(u => u.UserName == registerReq.Username)
                    .FirstOrDefaultAsync() != null)
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(),
                    "username is already exists."));

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

            // To be fixed
            //assign a role to the newly created User
            //var role = new ApplicationRole { Name = regRequest.UserType };
            //await _roleManager.CreateAsync(role);
            var role = await _roleManager.FindByNameAsync(registerReq.UserType);
            await _userManager.AddToRoleAsync(user, role.Name);

            switch (registerReq.UserType)
            {
                case Constants.USER_TYPE_FREELANCER:
                {
                    var createdUser = await _mainAppContext.Users.OfType<Freelancer>()
                        .Where(u => u.UserName == registerReq.Username)
                        .Select(u => new FreelancerResponseDTO()
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Username = u.UserName ?? string.Empty,
                            PhoneNumber = u.PhoneNumber ?? string.Empty,
                            Skills = u.Skills,
                            UserType = Constants.USER_TYPE_FREELANCER,
                            Role = new RoleResponseDTO { Id = role.Id, Name = role.Name }
                        })
                        .FirstOrDefaultAsync();
                    return Ok(CreateSuccessResponse(createdUser));
                }
                case Constants.USER_TYPE_CLIENT:
                {
                    var createdUser = await _mainAppContext.Users.OfType<Client>()
                        .Where(c => c.UserName == registerReq.Username)
                        .Select(c => new ClientResponseDTO
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Username = c.UserName ?? string.Empty,
                            PhoneNumber = c.PhoneNumber ?? string.Empty,
                            CompanyName = c.CompanyName,
                            UserType = Constants.USER_TYPE_CLIENT,
                            Role = new RoleResponseDTO { Id = role.Id, Name = role.Name }
                        })
                        .FirstOrDefaultAsync();
                    return Ok(CreateSuccessResponse(createdUser));
                }
                default:
                    return Ok();
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] AuthRequest req)
        {
            var user = await _userManager.FindByNameAsync(req.UserName);
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