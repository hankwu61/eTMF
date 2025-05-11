using System.Collections.Generic;

namespace ETMF.Models.ViewModels
{
    /// <summary>
    /// 首頁儀表板視圖模型
    /// </summary>
    public class DashboardViewModel
    {
        // 文檔統計
        public int TotalDocumentsCount { get; set; }
        public int PendingDocumentsCount { get; set; }
        public int ApprovedDocumentsCount { get; set; }
        public int RejectedDocumentsCount { get; set; }
        public int ArchivedDocumentsCount { get; set; }
        public int DraftDocumentsCount { get; set; }
        public int ExpiredDocumentsCount { get; set; }
        
        // 近期文檔
        public List<DocumentSummary> RecentDocuments { get; set; } = new List<DocumentSummary>();
        
        // 最熱門文檔類型
        public List<ArtifactSummary> TopArtifacts { get; set; } = new List<ArtifactSummary>();
        
        // 系統統計
        public int TotalZonesCount { get; set; }
        public int TotalSectionsCount { get; set; }
        public int TotalArtifactsCount { get; set; }
        public int TotalUsersCount { get; set; }
        
        // 存儲統計
        public long TotalStorageUsed { get; set; } // 以字節為單位
        
        // 今日活動
        public int TodayUploadsCount { get; set; }
        public int TodayUpdatesCount { get; set; }
    }
    
    /// <summary>
    /// 文檔摘要信息
    /// </summary>
    public class DocumentSummary
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ArtifactName { get; set; } = string.Empty;
        public int ArtifactId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string ZoneName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
    /// <summary>
    /// 文檔類型摘要信息
    /// </summary>
    public class ArtifactSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string ZoneName { get; set; } = string.Empty;
    }
} 