using AonFreelancing.Contexts;
using AonFreelancing.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Twilio.Types;

namespace AonFreelancing.Utilities
{
    public class OTPManager
    {
        private static Random _random = new Random();
        private readonly MainAppContext _mainAppContext;

        public OTPManager(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }
        public async Task<OTP> GenerateOtpAsync(string phoneNumber)
        {
            OTP otp = new OTP();
            int ExpireTimeInMinutes = 5;
            // Generate a random number between 0 and 999999
            // and Format the number to ensure it's always 6 digits
            otp.Code = _random.Next(0, (int)Math.Pow(10, 6)).ToString("D6");
            otp.PhoneNumber = phoneNumber;
            otp.CreatedAt = DateTime.Now;
            otp.ExpireAt = DateTime.Now.AddMinutes(ExpireTimeInMinutes);
            otp.IsUsed = false;

            await _mainAppContext.OTPs.AddAsync(otp);
            await _mainAppContext.SaveChangesAsync();
            return otp;
        }

        public async Task<bool> ExpiredAsync(string code)
        {
            var otp = await _mainAppContext.OTPs.FirstOrDefaultAsync(o => o.Code.Equals(code));
            if (otp is null)
                throw new Exception($"{code} Otp is not found");
            return DateTime.Now >= otp.ExpireAt;
        }

        public async Task <bool> VerifyAsync(string code)
        {
            var otp = await _mainAppContext.OTPs.FirstOrDefaultAsync(o => o.Code.Equals(code));
            if (otp is null)
                throw new Exception($"{code} Otp is not found");
            return otp.Code.Equals(code);
        }
        public async Task DisabledAsync(string code)
        {
            var otp = await _mainAppContext.OTPs.FirstOrDefaultAsync(o => o.Code.Equals(code));
            if (otp is null)
                throw new Exception($"{code} Otp is not found");
            otp.IsUsed = true;
            await _mainAppContext.SaveChangesAsync();
        }
    }
}
