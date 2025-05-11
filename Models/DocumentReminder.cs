using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETMF.Models
{
    public class DocumentReminder
    {
        public DocumentReminder()
        {
            ReminderType = string.Empty;
            NotificationRecipients = string.Empty;
        }
        
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("Document")]
        public int DocumentId { get; set; }
        
        public virtual Document? Document { get; set; }
        
        public DateTime ExpiryDate { get; set; }
        
        public int ReminderDaysBeforeExpiry { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(50)]
        public string ReminderType { get; set; } // Email, System, Both
        
        [StringLength(500)]
        public string NotificationRecipients { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? LastSentAt { get; set; }
    }
} 