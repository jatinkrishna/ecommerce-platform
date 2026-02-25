using System;

namespace Ecommerce.Shared.Common.DTOs.Auth
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public UserDTO User { get; set; }
    }
}
