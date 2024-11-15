using AonFreelancing.Models;
using AonFreelancing.Utilities;
using AonFreelancing.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace AonFreelancing.Utilities
{
    class VerificationManger
    {
        private readonly MainAppContext _mainAppContext;
        private readonly UserManager<User> _userManager;
        private readonly OTPManager _otpManager;
        public VerificationManger(
            UserManager<User> userManager,
            MainAppContext mainAppContext,
            OTPManager otpManager
        )
        {
            _mainAppContext = mainAppContext;
            _userManager = userManager;
            _otpManager = otpManager;
        }

        public async Task<OTP> SendCodeAsync(string phoneNumber)
        {
            // Check if the phone number associated with any user Or any temprory user
            var user = await _userManager.Users.Where(u => u.PhoneNumber == phoneNumber).FirstOrDefaultAsync();
            var TempUser = await _mainAppContext.TempUsers.Where(tu => tu.PhoneNumber == phoneNumber).FirstOrDefaultAsync();
            if( user == null && TempUser == null)
                throw new Exception ("Phone Number isn't associated with any user.");

            string otpCode = _otpManager.GenerateOtp();
            // persist the otp to the otps table
            OTP otp = new OTP()
            {
                Code = otpCode,
                PhoneNumber = phoneNumber,
                CreatedAt = DateTime.Now,
                // TO be fixed add Expiret time to configration 
                ExpireAt = DateTime.Now.AddMinutes(5),
            };

            await _mainAppContext.OTPs.AddAsync(otp);
            await _mainAppContext.SaveChangesAsync();

            //send the otp to the specified phone number
            await _otpManager.SendOTPAsync(otp.Code, otp.PhoneNumber);
            return otp;
        }
    }
}