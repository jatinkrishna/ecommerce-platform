using System;

namespace Ecommerce.Shared.Common.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }

        public ApiException(string message, int statusCode = 500) : base(message)
        {
            StatusCode = statusCode;
        }

        public ApiException(string message, int statusCode, Exception innerException) 
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
