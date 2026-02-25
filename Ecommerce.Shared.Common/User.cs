using System;
using System.Collections.Generic;

namespace Ecommerce.Shared.Common
{
    public class User
    {
        // Primary Key
        public Guid Id { get; set; }

        // Authentication
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        // Profile
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Authorization
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; } = true;

        // Token Management
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Audit Trail
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}