using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETMF.Models
{
    /// <summary>
    /// 代表eTMF系统中的实际文档
    /// </summary>
    public class Document
    {
        public Document()
        {
            Title = string.Empty;
            Description = string.Empty;
            Status = string.Empty;
            Author = string.Empty;
            DocumentIdentifier = string.Empty;
            Source = string.Empty;
            TrialIdentifier = string.Empty;
            Metadata = new List<Metadata>();
            Versions = new List<DocumentVersion>();
        }
        
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        
        // 外键 - 关联到Artifact
        [ForeignKey("Artifact")]
        public int ArtifactId { get; set; }
        
        // 导航属性 - 所属文档类型
        public virtual Artifact? Artifact { get; set; }
        
        // 文档可以有多个元数据
        public virtual ICollection<Metadata> Metadata { get; set; }
        
        // 文档可以有多个版本
        public virtual ICollection<DocumentVersion> Versions { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        // 文档状态：草稿、待审核、已批准、已退回、已过期等
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
        
        // 文档作者
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;
        
        // 上传者/创建者ID
        public int CreatedById { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        // 唯一文档标识符
        [StringLength(100)]
        public string DocumentIdentifier { get; set; }
        
        // 文档来源：本地上传、外部系统导入等
        [StringLength(50)]
        public string Source { get; set; } = string.Empty;
        
        // 当前版本号
        public int CurrentVersionId { get; set; }
        
        // 是否已归档
        public bool IsArchived { get; set; }
        
        // 归档日期
        public DateTime? ArchivedAt { get; set; }
        
        // 试验标识符（如有多个试验）
        [StringLength(50)]
        public string TrialIdentifier { get; set; } = string.Empty;
    }
} 