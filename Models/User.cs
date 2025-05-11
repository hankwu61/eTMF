using System;
using System.ComponentModel.DataAnnotations;

namespace ETMF.Models
{
    /// <summary>
    /// 系统用户
    /// </summary>
    public class User
    {
        public User()
        {
            Username = string.Empty;
            Email = string.Empty;
            PasswordHash = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
        }
        
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; }
        
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        // 是否是管理员
        public bool IsAdmin { get; set; }
        
        // 是否激活
        public bool IsActive { get; set; } = true;
        
        // 最后登录时间
        public DateTime? LastLogin { get; set; }
        
        // 创建时间
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // 更新时间
        public DateTime? UpdatedAt { get; set; }
    }
} 