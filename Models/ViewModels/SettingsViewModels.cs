using System;
using System.ComponentModel.DataAnnotations;

namespace ETMF.Models.ViewModels
{
    public class SystemSettingsViewModel
    {
        [Display(Name = "應用程序名稱")]
        [Required(ErrorMessage = "請輸入應用程序名稱")]
        public string ApplicationName { get; set; } = string.Empty;
        
        [Display(Name = "公司名稱")]
        [Required(ErrorMessage = "請輸入公司名稱")]
        public string CompanyName { get; set; } = string.Empty;
        
        [Display(Name = "技術支持郵箱")]
        [EmailAddress(ErrorMessage = "請輸入有效的郵箱地址")]
        public string SupportEmail { get; set; } = string.Empty;
        
        [Display(Name = "最大上傳文件大小 (MB)")]
        [Range(1, 100, ErrorMessage = "文件大小必須在1-100MB之間")]
        public int MaxUploadFileSize { get; set; }
        
        [Display(Name = "允許的文件擴展名")]
        public string AllowedFileExtensions { get; set; } = string.Empty;
        
        [Display(Name = "啟用審計日誌")]
        public bool EnableAuditLogging { get; set; }
        
        [Display(Name = "默認頁面大小")]
        [Range(5, 100, ErrorMessage = "頁面大小必須在5-100之間")]
        public int DefaultPageSize { get; set; }
        
        [Display(Name = "啟用郵件通知")]
        public bool EnableEmailNotifications { get; set; }
        
        [Display(Name = "SMTP服務器")]
        public string SmtpServer { get; set; } = string.Empty;
        
        [Display(Name = "SMTP端口")]
        [Range(1, 65535, ErrorMessage = "端口號必須在1-65535之間")]
        public int SmtpPort { get; set; }
        
        [Display(Name = "SMTP用戶名")]
        public string SmtpUsername { get; set; } = string.Empty;
        
        [Display(Name = "SMTP密碼")]
        [DataType(DataType.Password)]
        public string SmtpPassword { get; set; } = string.Empty;
        
        [Display(Name = "使用SSL")]
        public bool SmtpUseSsl { get; set; }
        
        [Display(Name = "系統主題")]
        public string SystemTheme { get; set; } = string.Empty;
    }
    
    public class BackupViewModel
    {
        [Display(Name = "備份描述")]
        [Required(ErrorMessage = "請輸入備份描述")]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "包含文檔文件")]
        public bool IncludeFiles { get; set; } = true;
    }
    
    public class RestoreViewModel
    {
        [Display(Name = "備份文件")]
        [Required(ErrorMessage = "請選擇備份文件")]
        public string BackupFile { get; set; } = string.Empty;
        
        [Display(Name = "確認恢復")]
        [Required(ErrorMessage = "請確認您要恢復系統")]
        public bool ConfirmRestore { get; set; }
    }
    
    public class LogViewModel
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
    
    public class AboutViewModel
    {
        public string ApplicationName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime BuildDate { get; set; }
        public string Copyright { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DatabaseInfo { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string FrameworkVersion { get; set; } = string.Empty;
    }
} 