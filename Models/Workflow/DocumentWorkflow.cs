using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETMF.Models
{
    public class DocumentWorkflow
    {
        public DocumentWorkflow()
        {
            Status = string.Empty;
            History = new List<WorkflowHistory>();
        }
        
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("Document")]
        public int DocumentId { get; set; }
        
        public virtual Document? Document { get; set; }
        
        [ForeignKey("Workflow")]
        public int WorkflowId { get; set; }
        
        public virtual Workflow? Workflow { get; set; }
        
        [ForeignKey("CurrentStep")]
        public int CurrentStepId { get; set; }
        
        public virtual WorkflowStep? CurrentStep { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } // InProgress, Completed, Rejected
        
        public DateTime StartedAt { get; set; } = DateTime.Now;
        
        public DateTime? CompletedAt { get; set; }
        
        public virtual ICollection<WorkflowHistory> History { get; set; }
    }
} 