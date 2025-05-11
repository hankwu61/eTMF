using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ETMF.Models
{
    public class Workflow
    {
        public Workflow()
        {
            Name = string.Empty;
            Description = string.Empty;
            Steps = new List<WorkflowStep>();
        }
        
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public ICollection<WorkflowStep> Steps { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
    }
} 