using System.Collections.Generic;

namespace ETMF.Models.ViewModels
{
    /// <summary>
    /// 代表TMF参考模型树状结构的视图模型
    /// </summary>
    public class TmfTreeViewModel
    {
        public List<ZoneNode> Zones { get; set; } = new List<ZoneNode>();
    }

    /// <summary>
    /// Zone节点（区域）
    /// </summary>
    public class ZoneNode
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<SectionNode> Sections { get; set; } = new List<SectionNode>();
        public int DocumentCount { get; set; } // 该区域下所有文档数量
    }

    /// <summary>
    /// Section节点（部分）
    /// </summary>
    public class SectionNode
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ZoneId { get; set; }
        public List<ArtifactNode> Artifacts { get; set; } = new List<ArtifactNode>();
        public int DocumentCount { get; set; } // 该部分下所有文档数量
    }

    /// <summary>
    /// Artifact节点（文档类型）
    /// </summary>
    public class ArtifactNode
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public bool IsRequired { get; set; }
        public bool IsEssential { get; set; }
        public List<DocumentNode> Documents { get; set; } = new List<DocumentNode>();
        public int DocumentCount { get; set; } // 该文档类型下文档数量
    }

    /// <summary>
    /// Document节点（文档）
    /// </summary>
    public class DocumentNode
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DocumentIdentifier { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public bool IsArchived { get; set; }
        public int ArtifactId { get; set; }
        public int VersionCount { get; set; } // 文档版本数量
    }
} 