using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AonFreelancing.Services
{
    public sealed class TwilioService
    {
        private readonly IConfiguration _configuration;
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _twilioNumber;
        private readonly string _contentSid;

        public TwilioService(IConfiguration configuration)
        {
            _configuration = configuration;
            _accountSid = _configuration["Twilio:AccountSid"] ?? "";
            _authToken = _configuration["Twilio:AuthToken"] ?? "";
            _twilioNumber = _configuration["Twilio:PhoneNumber"] ?? "";
            _contentSid = _configuration["Twilio:ContentSid"] ?? "";
        }

        public async Task SendOtpAsync(string phoneNumber, string otp)
        {
            TwilioClient.Init(_accountSid, _authToken);
            var message = await MessageResource.CreateAsync(
                body: $"Your OTP is {otp}",
                from: new PhoneNumber(_twilioNumber),
                to: new PhoneNumber(phoneNumber),
                contentSid: _contentSid
            );
        }
    }

}
