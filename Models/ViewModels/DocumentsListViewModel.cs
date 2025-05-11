using System.Collections.Generic;

namespace ETMF.Models.ViewModels
{
    /// <summary>
    /// 文檔列表視圖模型
    /// </summary>
    public class DocumentsListViewModel
    {
        /// <summary>
        /// 頁面標題
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 頁面副標題
        /// </summary>
        public string SubTitle { get; set; } = string.Empty;
        
        /// <summary>
        /// 圖標CSS類
        /// </summary>
        public string IconClass { get; set; } = string.Empty;
        
        /// <summary>
        /// 狀態過濾條件
        /// </summary>
        public string StatusFilter { get; set; } = string.Empty;
        
        /// <summary>
        /// 文檔列表
        /// </summary>
        public List<Document> Documents { get; set; } = new List<Document>();
    }
} 