using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AonFreelancing.Services
{
    public sealed class TwilioService(IConfiguration configuration)
    {
        public async Task SendOtpAsync(string phoneNumber, string otp)
        {
            TwilioClient.Init(configuration["Twilio:Sid"], configuration["Twilio:Token"]);

            var messageOptions = new CreateMessageOptions(new PhoneNumber($"whatsapp:{phoneNumber}"))
            {
                From = new PhoneNumber(configuration["Twilio:From"]),
                ContentSid = configuration["Twilio:ContentSid"],
                ContentVariables = "{\"1\":\"" + otp + "\"}"
            };

            var message = await MessageResource.CreateAsync(messageOptions);
        }

        public async Task SendForgotPasswordAsync(string phoneNumber, string data)
        {
            TwilioClient.Init(configuration["Twilio:Sid"], configuration["Twilio:Token"]);

            var messageOptions = new CreateMessageOptions(new PhoneNumber($"whatsapp:{phoneNumber}"))
            {
                From = new PhoneNumber(configuration["Twilio:From"]),
                ContentSid = configuration["Twilio:ContentSid"],
                ContentVariables = "{\"1\":\"" + data + "\"}"
            };

            var resource = await MessageResource.CreateAsync(messageOptions);
        }
    }

}
