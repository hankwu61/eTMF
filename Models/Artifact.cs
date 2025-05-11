using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETMF.Models
{
    /// <summary>
    /// 代表DIA TMF参考模型中的文档类型（Artifact）
    /// 例如: 在部分01-试验计划下，可能有01-临床试验方案、02-方案修订、03-统计分析计划等文档类型
    /// </summary>
    public class Artifact
    {
        public Artifact()
        {
            Number = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            ArtifactCode = string.Empty;
            Documents = new List<Document>();
        }
        
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Number { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        // 外键 - 关联到Section
        [ForeignKey("Section")]
        public int SectionId { get; set; }
        
        // 导航属性 - 所属部分
        public virtual Section? Section { get; set; }
        
        // 一个文档类型可以包含多个实际文档
        public virtual ICollection<Document> Documents { get; set; }
        
        [StringLength(50)]
        public string ArtifactCode { get; set; } = string.Empty;
        
        public bool IsRequired { get; set; }
        
        public bool IsEssential { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
    }
} 