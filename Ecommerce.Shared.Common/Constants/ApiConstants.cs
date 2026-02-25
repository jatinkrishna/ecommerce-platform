using System;

namespace Ecommerce.Shared.Common.Constants
{
    public static class ApiConstants
    {
        public const string ApiVersion = "v1";
        public const string ApiPrefix = "api";
        
        public static class Auth
        {
            public const string Login = "auth/login";
            public const string Register = "auth/register";
            public const string RefreshToken = "auth/refresh-token";
            public const string Logout = "auth/logout";
        }
        
        public static class ContentTypes
        {
            public const string Json = "application/json";
            public const string Xml = "application/xml";
        }
        
        public static class Headers
        {
            public const string Authorization = "Authorization";
            public const string Bearer = "Bearer";
        }
    }
}
