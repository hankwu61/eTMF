using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETMF.Models
{
    public class WorkflowStep
    {
        public WorkflowStep()
        {
            Name = string.Empty;
            ApproverRoleId = string.Empty;
            ActionType = string.Empty;
        }
        
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("Workflow")]
        public int WorkflowId { get; set; }
        
        public virtual Workflow? Workflow { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        public int Order { get; set; }
        
        [StringLength(50)]
        public string ApproverRoleId { get; set; }
        
        [StringLength(50)]
        public string ActionType { get; set; } // Review, Approve, Reject
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 