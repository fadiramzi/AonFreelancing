using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.Requests;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace AonFreelancing.Services
{
    public class AuthService
    {
        private readonly MainAppContext _mainAppContext;
        public AuthService(MainAppContext mainAppContext) {
            _mainAppContext = mainAppContext;
        }

        public async Task<bool> IsUserExistsInTempAsync(PhoneNumberReq phoneNumberReq)
        {
            return await _mainAppContext.TempUsers
              .AnyAsync(u => u.PhoneNumber == phoneNumberReq.PhoneNumber);
        }

        public async Task AddAsync(TempUser tempUser)
        {
            await _mainAppContext.TempUsers.AddAsync(tempUser);
            await _mainAppContext.SaveChangesAsync();

        }
        public async Task AddOtpAsync(OTP oTP)
        {
            await _mainAppContext.OTPs.AddAsync(oTP);
            await _mainAppContext.SaveChangesAsync();
        }

        public async Task Remove(object item)
        {
            _mainAppContext.Remove(item);
            await _mainAppContext.SaveChangesAsync();
        }
        public async Task<TempUser> GetTempUserAsync(string PhoneNumber)
        {
            return await _mainAppContext.TempUsers.FirstOrDefaultAsync(u=>u.PhoneNumber == PhoneNumber);
        }
        public async Task<bool> IsUserValidForConfirmationAsync(string PhonerNumber)
        {
           return await _mainAppContext.TempUsers.AnyAsync(u => u.PhoneNumber == PhonerNumber && !u.PhoneNumberConfirmed);

        }

        public async Task<OTP> GetOTPAsync(string Phone)
        {
            return await _mainAppContext.OTPs.Where(o => o.PhoneNumber == Phone)
                   .FirstOrDefaultAsync();

        }

        public bool VerifyOtp(string ProvidedOTP, OTP otp)
        {
            if(otp != null && ProvidedOTP.Equals(otp.Code) && DateTime.Now < otp.ExpiresAt && otp.IsUsed == false)
            {
                return true;
            }

            return false;
        }

        public async Task UpdateTempUser(string PhoneNumber)
        {
            var TempUser = await _mainAppContext.TempUsers
              .FirstOrDefaultAsync(u => u.PhoneNumber == PhoneNumber);
            if (TempUser != null) { 
                TempUser.PhoneNumberConfirmed = true;
                await _mainAppContext.SaveChangesAsync();
            }
        }

        public async Task UpdateOtpAsync(OTP otp)
        {

            otp.IsUsed = true;// Update Otp
            await _mainAppContext.SaveChangesAsync();

        }

    }
}
