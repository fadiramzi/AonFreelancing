using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Requests;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly JwtService _jwtService;
        private readonly TwilioService _twitterService;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AuthController(
            UserManager<User> userManager,
            MainAppContext mainAppContext,
            IConfiguration configuration,
            JwtService jwtService,
            TwilioService twilioService,
            RoleManager<ApplicationRole> roleManager
            )
        {
            _userManager = userManager;
            _mainAppContext = mainAppContext;
            _configuration = configuration;
            _jwtService = jwtService;
            _twitterService = twilioService;
            _roleManager = roleManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegRequest req)
        {
            User user = req.UserType switch
            {
                Constants.USER_TYPE_FREELANCER => new Freelancer
                {
                    Name = req.Name,
                    UserName = req.Username,
                    PhoneNumber = req.PhoneNumber,
                    Skills = req.Skills ?? string.Empty
                },
                Constants.USER_TYPE_CLIENT => new Models.Client
                {
                    Name = req.Name,
                    UserName = req.Username,
                    PhoneNumber = req.PhoneNumber,
                    CompanyName = req.CompanyName ?? string.Empty
                },
                _ => new SystemUser()
            };

            bool isUsernameTaken = await _userManager.Users.AnyAsync(u => u.UserName == req.Username);

            if (isUsernameTaken)
            {
                return BadRequest(new ApiResponse<string>()
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = new List<Error>() {
                        new Error() {
                            Code = StatusCodes.Status400BadRequest.ToString(),
                            Message = "Username is already taken."
                        }
                    }
                });
            }

            var result = await _userManager.CreateAsync(user, req.Password);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Results = null,
                    Errors = result.Errors
                    .Select(e => new Error()
                    {
                        Code = e.Code,
                        Message = e.Description
                    })
                    .ToList()
                });

            }

            var role = new ApplicationRole { Name = req.UserType };
            await _roleManager.CreateAsync(role);
            await _userManager.AddToRoleAsync(user, role.Name);

            var code = OTPManager.GenerateOtp();
            var otp = new Otp
            {
                Code = code,
                PhoneNumber = req.PhoneNumber,
                CreatedDate = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(1)
            };

            await _mainAppContext.Otps.AddAsync(otp);
            await _mainAppContext.SaveChangesAsync();

            await _twitterService.SendOtpAsync(otp.PhoneNumber, otp.Code);


            var createdUser = req.UserType switch
            {
                Constants.USER_TYPE_FREELANCER => await _mainAppContext.Users.OfType<Freelancer>()
                                .Where(u => u.UserName == req.Username)
                                .Select(u => new FreelancerResponseDTO()
                                {
                                    Id = u.Id,
                                    Name = u.Name,
                                    Username = u.UserName ?? string.Empty,
                                    PhoneNumber = u.PhoneNumber ?? string.Empty,
                                    Skills = u.Skills,
                                    UserType = Constants.USER_TYPE_FREELANCER,
                                    Role = new RoleResponseDTO { Id = role.Id ,Name = role.Name }

                                })
                                .FirstOrDefaultAsync() as UserResponseDTO,
                Constants.USER_TYPE_CLIENT => await _mainAppContext.Users.OfType<Models.Client>()
                    .Where(u => u.UserName == req.Username)
                    .Select(u => new ClientResponseDTO()
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Username = u.UserName ?? string.Empty,
                        PhoneNumber = u.PhoneNumber ?? string.Empty,
                        CompanyName = u.CompanyName,
                        UserType = Constants.USER_TYPE_CLIENT,
                        Role = new RoleResponseDTO { Id = role.Id, Name = role.Name }

                    })
                    .FirstOrDefaultAsync() as UserResponseDTO,
                _ => null
            };


            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Errors = [],
                Results = createdUser ?? new UserResponseDTO()
            });



        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] AuthRequest req)
        {
            var user = await _userManager.FindByNameAsync(req.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, req.Password))
            {
                if (!await _userManager.IsPhoneNumberConfirmedAsync(user))
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        IsSuccess = false,
                        Errors = new List<Error>() {
                            new Error(){
                            Code = StatusCodes.Status401Unauthorized.ToString(),
                            Message = "Verify Your Account First"
                            }
                        }
                    });
                }

                string userType = user switch
                {
                    Freelancer    => Constants.USER_TYPE_FREELANCER,
                    Models.Client => Constants.USER_TYPE_CLIENT,
                    SystemUser    => Constants.USER_TYPE_SYSTEM_USER,

                    _ => "Unknown"
                };
               
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                var token = _jwtService.CreateToken(user, role);

                var userResponse = new UserResponseDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    Username = user.UserName ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    IsPhoneNumberVerified = user.PhoneNumberConfirmed,
                    UserType = userType,
                    Role = new RoleResponseDTO { Name = role }
                };
           
                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    Errors = [],
                    Results = new
                    {
                        UserDetails = userResponse,
                        AccessToken = token
                    }

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
        public async Task<IActionResult> VerifyAsync([FromBody] VerifyReq req)
        {
            var user = await _userManager.Users.Where(x => x.PhoneNumber == req.Phone).FirstOrDefaultAsync();
            if (user != null && !await _userManager.IsPhoneNumberConfirmedAsync(user))
            {
                var otp = await _mainAppContext.Otps.Where(o => o.PhoneNumber == req.Phone)
                    .OrderByDescending(o => o.CreatedDate)
                    .FirstOrDefaultAsync();

                if (otp != null && req.Otp.Equals(otp.Code) && DateTime.Now < otp.ExpiresAt)
                {
                    user.PhoneNumberConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    
                    otp.IsUsed = true;
                    await _mainAppContext.SaveChangesAsync();

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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordReq req)
        {
            var user = await _userManager.Users.Where(u => u.PhoneNumber == req.PhoneNumber).FirstOrDefaultAsync();
            if (user != null && user.PhoneNumber != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _twitterService.SendFogotPassowrdAsync(user.PhoneNumber, token);
            }
            return Ok(new ApiResponse<string>
            {
                IsSuccess = true,
                Results = "If the phone number is registered, you will receive an OTP.",
                Errors = []
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordReq req)
        {
            var user = await _userManager.Users.Where(u => u.PhoneNumber == req.PhoneNumber).FirstOrDefaultAsync();
            if (user != null && user.PhoneNumber != null 
                && req.Token != null && req.Password != null)
            {
                var result = await _userManager.ResetPasswordAsync(user, req.Token, req.Password);
                if (result.Succeeded)
                {
                    return Ok(new ApiResponse<string>
                    {
                        IsSuccess = true,
                        Results = "Password reset successfully.",
                        Errors = []
                    });
                }
            }
            return BadRequest(new ApiResponse<string>
            {
                IsSuccess = false,
                Results = null,
                Errors = new List<Error>() {
                    new Error(){
                        Code = StatusCodes.Status400BadRequest.ToString(),
                        Message = "Password reset failed."
                    }
                }
            });
        }
    }
}
