

using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace AonFreelancing.Utilities
{
    class TempUserManger
    {
        private readonly MainAppContext _mainAppContext;
        private readonly UserManager<User> _userManager;
        public TempUserManger(
            UserManager<User> userManager,
            MainAppContext mainAppContext
        )
        {
            _mainAppContext = mainAppContext;
            _userManager = userManager;
        }
        public async Task<TempUser> CreateOrUpdateTempUserAsync(TempUserDTO tempUserDto)
        {
            // Check if the phone number associated with any user Or any temprory user
            var user = await _userManager.Users.Where(u => u.PhoneNumber == tempUserDto.PhoneNumber).FirstOrDefaultAsync();
            var TempUser = await _mainAppContext.TempUsers.Where(tu => tu.PhoneNumber == tempUserDto.PhoneNumber).FirstOrDefaultAsync();
            
            // if it's taken by a primary User return a bad request
            if (user != null)
                throw new Exception("phone number is already used by an account");

            // if there is a TempUser user then update it with the new TempUser
            if(TempUser != null)
            {
                TempUser.UserType = tempUserDto.UserType;
                _mainAppContext.TempUsers.Update(TempUser);
            }
            // else create a new TempUser
            else
            {
                TempUser = new TempUser()
                {
                    UserType = tempUserDto.UserType,
                    PhoneNumber = tempUserDto.PhoneNumber,
                    verified = false
                };
                await _mainAppContext.TempUsers.AddAsync(TempUser);
            }

            // save the temporary user
            await _mainAppContext.SaveChangesAsync();
            return TempUser;
        }
        public async Task RemoveTempUser(TempUser tempUser)
        {
            throw new NotImplementedException();
        }
    }
}