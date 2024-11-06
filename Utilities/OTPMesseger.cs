using AonFreelancing.Contexts;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authentication;

namespace AonFreelancing.Utilities
{
    public class OTPMesseger
    {
        private readonly IConfiguration _configuration;
        private readonly MainAppContext _mainAppContext;

        public OTPMesseger(IConfiguration configuration, MainAppContext mainAppContext)
        {
            _configuration = configuration;
            _mainAppContext = mainAppContext;
        }
        public async Task SendOTPAsync(string PhoneNumber)
        {
            var from = _configuration["Twilio:From"];
            var to = _configuration["Twilio:To"]; // for testing 
            var accountSid = _configuration["Twilio:Sid"];
            var authToken = _configuration["Twilio:Token"];
            OTPManager otpManger = new OTPManager(_mainAppContext);
            var otp = await otpManger.GenerateOtpAsync(PhoneNumber);
            TwilioClient.Init(accountSid, authToken);

            var messageOptions = new CreateMessageOptions(
                new PhoneNumber($"whatsapp:{to}")); //Usign default to for testing
            messageOptions.From = new PhoneNumber($"whatsapp:{from}");
            messageOptions.ContentSid = _configuration["Twilio:ContentSid"];
            messageOptions.ContentVariables = "{\"1\":\"" + otp.Code + "\"}";

            MessageResource.Create(messageOptions);
        }
    }

}
