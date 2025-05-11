using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ETMF.Models
{
    /// <summary>
    /// 代表DIA TMF参考模型中的区域（Zone）
    /// 例如: 01-临床研究、02-监管事务等
    /// </summary>
    public class Zone
    {
        public Zone()
        {
            Number = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            Sections = new List<Section>();
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
        
        // 导航属性 - 一个区域可以有多个部分
        public virtual ICollection<Section> Sections { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
    }
} 