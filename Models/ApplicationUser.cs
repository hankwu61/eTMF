using Microsoft.AspNetCore.Identity;
using System;

namespace ETMF.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // 获取用户全名
        public string FullName => $"{FirstName} {LastName}";
    }
} 