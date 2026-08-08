using Microsoft.AspNetCore.Http; 

namespace TaskMangment.Application.Common.Exceptions
{
    public class AppException : Exception
    {
        public string ErrorCode { get; }
        public int StatusCode { get; }

        public AppException(
            string errorCode,
            int statusCode = StatusCodes.Status400BadRequest,
            string? detail = null)
            : base(detail ?? errorCode)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }

}
