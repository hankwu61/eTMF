using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETMF.Models
{
    /// <summary>
    /// 代表DIA TMF参考模型中的部分（Section）
    /// 例如: 在01-临床研究区域下，可能有01-试验计划、02-试验方案等部分
    /// </summary>
    public class Section
    {
        public Section()
        {
            Number = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            Artifacts = new List<Artifact>();
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
        
        // 外键 - 关联到Zone
        [ForeignKey("Zone")]
        public int ZoneId { get; set; }
        
        // 导航属性 - 所属区域
        public virtual Zone? Zone { get; set; }
        
        // 导航属性 - 一个部分可以包含多个文档类型
        public virtual ICollection<Artifact> Artifacts { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
    }
} 