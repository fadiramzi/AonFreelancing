using AonFreelancing.Models;
using Microsoft.AspNetCore.Mvc;
using Twilio.TwiML.Messaging;

namespace AonFreelancing.Controllers
{
    public class BaseController : ControllerBase
    {
        protected ApiResponse<T> CreateSuccessResponse<T>(T data)
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Results = data
            };
        }

        protected ApiResponse<object> CreateErrorResponse(string code, string message)
        {
            return new ApiResponse<object>
            {
                Errors = [new Error { Code = code, Message = message }]
            };
        }
       
    }
}