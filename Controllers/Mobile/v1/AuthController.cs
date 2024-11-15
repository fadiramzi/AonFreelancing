using System.Data;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
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
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.IdentityModel.Tokens;

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

        [Route("SendVerificationCode")]
        [HttpPost]
        public async Task<IActionResult> SendVerificationCodeAsync([FromBody] TempUserDTO tempUserDTO)
        {
            var tempUserManger = new TempUserManger(_userManager, _mainAppContext);
            var tempUser = await tempUserManger.CreateOrUpdateTempUserAsync(tempUserDTO);
            var verificationManger = new VerificationManger(_userManager, _mainAppContext, _otpManager);

            try
            {
                OTP otp = await verificationManger.SendCodeAsync(tempUser.PhoneNumber);
            }
            catch(Exception error)
            {
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), error.Message));
            }
            
            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegRequest regRequest)
        {
            var tempUser = await _mainAppContext.TempUsers.FirstOrDefaultAsync(ut => ut.PhoneNumber == regRequest.PhoneNumber && ut.Verified == true);
            // Check if tempUser exiset and it's verified
            if (tempUser == null || !tempUser.Verified)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), $"{regRequest.PhoneNumber} is't verified."));
           
            // check if Email or phoneNumber isn't taken
            if (await _userManager.Users.Where(u => u.Email == regRequest.Email || u.PhoneNumber == regRequest.PhoneNumber).FirstOrDefaultAsync() != null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "Email or Phon number is already used by an account"));
            
             // Register User
            if (tempUser.UserType == Constants.USER_TYPE_FREELANCER)
            {
                Freelancer user = new Freelancer()                
                {
                    Name = regRequest.Name,
                    Email = regRequest.Email,
                    PhoneNumber = tempUser.PhoneNumber,
                    UserType = tempUser.UserType,
                    UserName = regRequest.Email
                };
                var userCreationResult = await _userManager.CreateAsync(user, regRequest.Password);
                return Ok(CreateSuccessResponse<UserOutDTO>(new UserOutDTO()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                }));
            }
            else
            {
                Client user = new Client()
                {
                    Name = regRequest.Name,
                    Email = regRequest.Email,
                    PhoneNumber = tempUser.PhoneNumber,
                    UserType = tempUser.UserType,
                    UserName = regRequest.Email
                };
                // Create new User with hashed passworrd in the database
                var userCreationResult = await _userManager.CreateAsync(user, regRequest.Password);
                return Ok(CreateSuccessResponse<UserOutDTO>(new UserOutDTO()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                }));
            }

            // To be fixed
            // //assign a role to the newly created User
            // var role = new ApplicationRole { Name = regRequest.UserType };
            // await _roleManager.CreateAsync(role);
            // var role = await _roleManager.FindByNameAsync(regRequest.UserType);
            // await _userManager.AddToRoleAsync(user, role.Name);

            
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
                var token = _jwtService.GenerateJWT(user, role);
                return Ok(CreateSuccessResponse(new LoginResponse()
                {
                    AccessToken = token,
                    UserDetailsDTO = new UserDetailsDTO(user, role)
                }));
            }

            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "UnAuthorized"));
        }

        [HttpPost("VerifyPhoneNumber")]
        public async Task<IActionResult> VerifyPhoneNumberAsync([FromBody] VerifyReq verifyReq)
        {
            var tempUser = await _mainAppContext.TempUsers.FirstOrDefaultAsync(tu => tu.PhoneNumber == verifyReq.PhoneNumber);
            // if user exist 
            if (tempUser == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), $"{verifyReq.PhoneNumber} isn't associated with any user."));

            OTP? otp = await _mainAppContext.OTPs.Where(o => o.PhoneNumber == verifyReq.PhoneNumber).FirstOrDefaultAsync();

            // verify OTP
            if (otp == null || !verifyReq.Otp.Equals(otp.Code))
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), $"Invalid OTP."));
            if (DateTime.Now > otp.ExpireAt)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), $"OTP is expired."));

            // else verify otp
            tempUser.Verified = true;
            otp.IsUsed = true;
            
            _mainAppContext.OTPs.Remove(otp);
            await _mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Verified"));
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
