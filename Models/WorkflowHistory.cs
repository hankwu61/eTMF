using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETMF.Models
{
    public class WorkflowHistory
    {
        [Key]
        public int Id { get; set; }
        
        // 关联的文档工作流
        [ForeignKey("DocumentWorkflow")]
        public int DocumentWorkflowId { get; set; }
        
        public virtual DocumentWorkflow? DocumentWorkflow { get; set; }
        
        // 步骤信息
        public int StepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        
        // 动作
        public string Action { get; set; } = string.Empty; // Approved, Rejected, Returned, Comment
        
        // 批注
        public string Comment { get; set; } = string.Empty;
        
        // 操作人
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        
        // 时间戳
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 