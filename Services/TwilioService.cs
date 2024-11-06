using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AonFreelancing.Services
{
    public sealed class TwilioService
    {
        private readonly IConfiguration _configuration;

        public TwilioService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpAsync(string phoneNumber, string data)
        {
            TwilioClient.Init(_configuration["Twilio:Sid"], _configuration["Twilio:Token"]);

            var messageOptions = new CreateMessageOptions(new PhoneNumber($"whatsapp:{phoneNumber}"));
            messageOptions.From = new PhoneNumber(_configuration["Twilio:From"]);
            messageOptions.ContentSid = _configuration["Twilio:ContentSid"];
            messageOptions.ContentVariables = "{\"1\":\"" + data + "\"}";

            var message = await MessageResource.CreateAsync(messageOptions);
        }
    }

}
