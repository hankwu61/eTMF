using System.ComponentModel.DataAnnotations;

namespace ETMF.Models
{
    public class SystemSettings
    {
        [Required(ErrorMessage = "请输入系统名称")]
        [Display(Name = "系统名称")]
        public string SystemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "请输入文档保留天数")]
        [Range(1, 3650, ErrorMessage = "文档保留天数必须在1-3650天之间")]
        [Display(Name = "文档保留天数")]
        public int DocumentRetentionDays { get; set; }

        [Required(ErrorMessage = "请输入最大文件大小")]
        [Range(1, 1000, ErrorMessage = "最大文件大小必须在1-1000MB之间")]
        [Display(Name = "最大文件大小")]
        public int MaxFileSize { get; set; }

        [Required(ErrorMessage = "请输入允许的文件类型")]
        [Display(Name = "允许的文件类型")]
        public string AllowedFileTypes { get; set; } = string.Empty;

        [Display(Name = "启用邮件通知")]
        public bool EmailNotificationEnabled { get; set; }

        [Display(Name = "SMTP服务器")]
        public string SmtpServer { get; set; } = string.Empty;

        [Display(Name = "SMTP端口")]
        public int SmtpPort { get; set; }

        [Display(Name = "SMTP用户名")]
        public string SmtpUsername { get; set; } = string.Empty;

        [Display(Name = "SMTP密码")]
        [DataType(DataType.Password)]
        public string? SmtpPassword { get; set; }
    }
} 