using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETMF.Models
{
    /// <summary>
    /// 文档版本
    /// </summary>
    public class DocumentVersion
    {
        public DocumentVersion()
        {
            FileName = string.Empty;
            FilePath = string.Empty;
            FileType = string.Empty;
            FileHash = string.Empty;
            VersionComment = string.Empty;
            MimeType = string.Empty;
        }
        
        [Key]
        public int Id { get; set; }
        
        // 外键 - 关联到Document
        [ForeignKey("Document")]
        public int DocumentId { get; set; }
        
        // 导航属性 - 所属文档
        public virtual Document? Document { get; set; }
        
        // 版本号
        [Required]
        public int VersionNumber { get; set; }
        
        // 文件名
        [Required]
        [StringLength(255)]
        public string FileName { get; set; }
        
        // 文件路径
        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }
        
        // 文件大小（字节）
        public long FileSize { get; set; }
        
        // 文件类型/扩展名
        [StringLength(50)]
        public string FileType { get; set; } = string.Empty;
        
        // 文件哈希值（用于验证文件完整性）
        [StringLength(128)]
        public string FileHash { get; set; } = string.Empty;
        
        // 版本说明
        [StringLength(500)]
        public string VersionComment { get; set; } = string.Empty;
        
        // 上传者/创建者ID
        public int UploadedById { get; set; }
        
        // 上传/创建时间
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // 是否是当前版本
        public bool IsCurrent { get; set; }
        
        // MIME类型
        [StringLength(100)]
        public string MimeType { get; set; } = string.Empty;
        
        // 页数（适用于PDF等文档）
        public int? PageCount { get; set; }
    }
} 