using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETMF.Models
{
    /// <summary>
    /// 文档元数据
    /// </summary>
    public class Metadata
    {
        public Metadata()
        {
            Name = string.Empty;
            Value = string.Empty;
            DataType = string.Empty;
        }
        
        [Key]
        public int Id { get; set; }
        
        // 外键 - 关联到Document
        [ForeignKey("Document")]
        public int DocumentId { get; set; }
        
        // 导航属性 - 所属文档
        public virtual Document? Document { get; set; }
        
        // 元数据名称
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        // 元数据值
        [Required]
        [StringLength(500)]
        public string Value { get; set; }
        
        // 数据类型：Text, Number, Date, Boolean等
        [StringLength(50)]
        public string DataType { get; set; } = string.Empty;
        
        // 是否是系统生成的元数据
        public bool IsSystemGenerated { get; set; }
        
        // 是否可搜索
        public bool IsSearchable { get; set; }
        
        // 是否在UI中显示
        public bool IsDisplayed { get; set; }
        
        // 创建时间
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // 更新时间
        public DateTime? UpdatedAt { get; set; }
    }
} 