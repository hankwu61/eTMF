using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETMF.Models
{
    /// <summary>
    /// 系统审计日志
    /// </summary>
    public class AuditLog
    {
        public AuditLog()
        {
            ActionType = string.Empty;
            EntityType = string.Empty;
            Description = string.Empty;
            IpAddress = string.Empty;
            UserAgent = string.Empty;
            ErrorMessage = string.Empty;
        }
        
        [Key]
        public int Id { get; set; }
        
        // 用户ID
        public int? UserId { get; set; }
        
        // 导航属性 - 执行操作的用户
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        // 操作类型：创建、修改、删除、查看、下载等
        [Required]
        [StringLength(50)]
        public string ActionType { get; set; }
        
        // 操作对象：Document, User, Zone等
        [Required]
        [StringLength(50)]
        public string EntityType { get; set; }
        
        // 操作对象的ID
        public int? EntityId { get; set; }
        
        // 操作详情
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        // IP地址
        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;
        
        // 用户代理（浏览器信息）
        [StringLength(500)]
        public string UserAgent { get; set; } = string.Empty;
        
        // 操作时间
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        // 是否成功
        public bool IsSuccess { get; set; }
        
        // 错误信息（如果操作失败）
        [StringLength(500)]
        public string ErrorMessage { get; set; } = string.Empty;
    }
} 