using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ETMF.Models;
using ETMF.Data;
using Microsoft.EntityFrameworkCore;
using ETMF.Models.ViewModels;
using System.Linq;
using System;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ETMF.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        ILogger<HomeController> logger, 
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var dashboard = new DashboardViewModel();
        
        try
        {
            // 文檔狀態統計
            dashboard.TotalDocumentsCount = await _context.Documents.CountAsync();
            dashboard.PendingDocumentsCount = await _context.Documents.CountAsync(d => d.Status == "待審核");
            dashboard.ApprovedDocumentsCount = await _context.Documents.CountAsync(d => d.Status == "已批准");
            dashboard.RejectedDocumentsCount = await _context.Documents.CountAsync(d => d.Status == "已退回");
            dashboard.ArchivedDocumentsCount = await _context.Documents.CountAsync(d => d.IsArchived);
            dashboard.DraftDocumentsCount = await _context.Documents.CountAsync(d => d.Status == "草稿");
            dashboard.ExpiredDocumentsCount = await _context.Documents.CountAsync(d => d.Status == "已過期");
            
            // 取得最近上傳的10個文檔
            var recentDocuments = await _context.Documents
                .Include(d => d.Artifact)
                    .ThenInclude(a => a != null ? a.Section : null)
                        .ThenInclude(s => s != null ? s.Zone : null)
                .OrderByDescending(d => d.CreatedAt)
                .Take(10)
                .ToListAsync();
                
            dashboard.RecentDocuments = recentDocuments.Select(d => new DocumentSummary
            {
                Id = d.Id,
                Title = d.Title,
                Status = d.Status,
                Author = d.Author,
                ArtifactName = d.Artifact?.Name ?? string.Empty,
                ArtifactId = d.ArtifactId,
                SectionName = d.Artifact?.Section?.Name ?? string.Empty,
                ZoneName = d.Artifact?.Section?.Zone?.Name ?? string.Empty,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            }).ToList();
            
            // 取得文檔數量最多的5個文檔類型
            var topArtifacts = await _context.Artifacts
                .Include(a => a.Section)
                    .ThenInclude(s => s != null ? s.Zone : null)
                .Include(a => a.Documents)
                .OrderByDescending(a => a.Documents.Count)
                .Take(5)
                .ToListAsync();
                
            dashboard.TopArtifacts = topArtifacts.Select(a => new ArtifactSummary
            {
                Id = a.Id,
                Name = a.Name,
                DocumentCount = a.Documents.Count,
                SectionName = a.Section?.Name ?? string.Empty,
                ZoneName = a.Section?.Zone?.Name ?? string.Empty
            }).ToList();
            
            // 系統統計
            dashboard.TotalZonesCount = await _context.Zones.CountAsync();
            dashboard.TotalSectionsCount = await _context.Sections.CountAsync();
            dashboard.TotalArtifactsCount = await _context.Artifacts.CountAsync();
            dashboard.TotalUsersCount = await _userManager.Users.CountAsync();
            
            // 存儲統計
            var versions = await _context.DocumentVersions.ToListAsync();
            dashboard.TotalStorageUsed = versions.Sum(v => v.FileSize);
            
            // 今日活動
            var today = DateTime.Today;
            dashboard.TodayUploadsCount = await _context.Documents.CountAsync(d => d.CreatedAt.Date == today);
            dashboard.TodayUpdatesCount = await _context.DocumentVersions.CountAsync(v => v.CreatedAt.Date == today && v.VersionNumber > 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得儀表板數據時出錯");
        }
        
        return View(dashboard);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    /// <summary>
    /// 显示TMF参考模型树状结构视图
    /// </summary>
    [Authorize]
    public async Task<IActionResult> TmfTree()
    {
        var viewModel = new TmfTreeViewModel();
        
        // 获取当前用户信息
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            ViewBag.AuthorName = $"{user.FirstName} {user.LastName}".Trim();
        }
        
        // 获取所有Zone（包含关联的Section、Artifact和Document）
        var zones = await _context.Zones
            .Include(z => z.Sections)
                .ThenInclude(s => s.Artifacts)
                    .ThenInclude(a => a.Documents)
                        .ThenInclude(d => d.Versions)
            .OrderBy(z => z.Number)
            .ToListAsync();
            
        // 转换为视图模型
        foreach (var zone in zones)
        {
            var zoneNode = new ZoneNode
            {
                Id = zone.Id,
                Number = zone.Number,
                Name = zone.Name,
                Description = zone.Description,
                DocumentCount = zone.Sections.SelectMany(s => s.Artifacts).SelectMany(a => a.Documents).Count()
            };
            
            // 添加Section节点
            foreach (var section in zone.Sections.OrderBy(s => s.Number))
            {
                var sectionNode = new SectionNode
                {
                    Id = section.Id,
                    Number = section.Number,
                    Name = section.Name,
                    Description = section.Description,
                    ZoneId = zone.Id,
                    DocumentCount = section.Artifacts.SelectMany(a => a.Documents).Count()
                };
                
                // 添加Artifact节点
                foreach (var artifact in section.Artifacts.OrderBy(a => a.Number))
                {
                    var artifactNode = new ArtifactNode
                    {
                        Id = artifact.Id,
                        Number = artifact.Number,
                        Name = artifact.Name,
                        Description = artifact.Description,
                        SectionId = section.Id,
                        IsRequired = artifact.IsRequired,
                        IsEssential = artifact.IsEssential,
                        DocumentCount = artifact.Documents.Count
                    };
                    
                    // 添加Document节点
                    foreach (var document in artifact.Documents.OrderBy(d => d.Title))
                    {
                        var documentNode = new DocumentNode
                        {
                            Id = document.Id,
                            Title = document.Title,
                            Status = document.Status,
                            DocumentIdentifier = document.DocumentIdentifier,
                            Author = document.Author,
                            IsArchived = document.IsArchived,
                            ArtifactId = artifact.Id,
                            VersionCount = document.Versions.Count
                        };
                        
                        artifactNode.Documents.Add(documentNode);
                    }
                    
                    sectionNode.Artifacts.Add(artifactNode);
                }
                
                zoneNode.Sections.Add(sectionNode);
            }
            
            viewModel.Zones.Add(zoneNode);
        }
        
        return View(viewModel);
    }
    
    /// <summary>
    /// 待審核文檔列表
    /// </summary>
    [Authorize(Policy = "CanViewDocuments")]
    public async Task<IActionResult> PendingDocuments()
    {
        var documents = await _context.Documents
            .Where(d => d.Status == "待審核")
            .Include(d => d.Artifact)
                .ThenInclude(a => a != null ? a.Section : null)
                    .ThenInclude(s => s != null ? s.Zone : null)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
            
        return View("DocumentsList", new DocumentsListViewModel
        {
            Documents = documents,
            Title = "待審核文檔",
            SubTitle = "需要審核的文檔列表",
            IconClass = "fa-clock",
            StatusFilter = "待審核"
        });
    }
    
    /// <summary>
    /// 已批准文檔列表
    /// </summary>
    [Authorize(Policy = "CanViewDocuments")]
    public async Task<IActionResult> ApprovedDocuments()
    {
        var documents = await _context.Documents
            .Where(d => d.Status == "已批准")
            .Include(d => d.Artifact)
                .ThenInclude(a => a != null ? a.Section : null)
                    .ThenInclude(s => s != null ? s.Zone : null)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync();
            
        return View("DocumentsList", new DocumentsListViewModel
        {
            Documents = documents,
            Title = "已批准文檔",
            SubTitle = "所有已批准的文檔",
            IconClass = "fa-check-circle",
            StatusFilter = "已批准"
        });
    }
    
    /// <summary>
    /// 草稿文檔列表
    /// </summary>
    [Authorize(Policy = "CanViewDocuments")]
    public async Task<IActionResult> DraftDocuments()
    {
        var documents = await _context.Documents
            .Where(d => d.Status == "草稿")
            .Include(d => d.Artifact)
                .ThenInclude(a => a != null ? a.Section : null)
                    .ThenInclude(s => s != null ? s.Zone : null)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
            
        return View("DocumentsList", new DocumentsListViewModel
        {
            Documents = documents,
            Title = "草稿文檔",
            SubTitle = "尚未提交審核的草稿文檔",
            IconClass = "fa-pencil-alt",
            StatusFilter = "草稿"
        });
    }
    
    /// <summary>
    /// 被退回文檔列表
    /// </summary>
    [Authorize(Policy = "CanViewDocuments")]
    public async Task<IActionResult> RejectedDocuments()
    {
        var documents = await _context.Documents
            .Where(d => d.Status == "已退回")
            .Include(d => d.Artifact)
                .ThenInclude(a => a != null ? a.Section : null)
                    .ThenInclude(s => s != null ? s.Zone : null)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync();
            
        return View("DocumentsList", new DocumentsListViewModel
        {
            Documents = documents,
            Title = "被退回文檔",
            SubTitle = "審核未通過或被退回的文檔",
            IconClass = "fa-times-circle",
            StatusFilter = "已退回"
        });
    }
    
    /// <summary>
    /// 已歸檔文檔列表
    /// </summary>
    [Authorize(Policy = "CanViewDocuments")]
    public async Task<IActionResult> ArchivedDocuments()
    {
        var documents = await _context.Documents
            .Where(d => d.IsArchived)
            .Include(d => d.Artifact)
                .ThenInclude(a => a != null ? a.Section : null)
                    .ThenInclude(s => s != null ? s.Zone : null)
            .OrderByDescending(d => d.ArchivedAt)
            .ToListAsync();
            
        return View("DocumentsList", new DocumentsListViewModel
        {
            Documents = documents,
            Title = "已歸檔文檔",
            SubTitle = "已歸檔的文檔列表",
            IconClass = "fa-archive",
            StatusFilter = "已歸檔"
        });
    }
    
    /// <summary>
    /// 已過期文檔列表
    /// </summary>
    [Authorize(Policy = "CanViewDocuments")]
    public async Task<IActionResult> ExpiredDocuments()
    {
        var documents = await _context.Documents
            .Where(d => d.Status == "已過期")
            .Include(d => d.Artifact)
                .ThenInclude(a => a != null ? a.Section : null)
                    .ThenInclude(s => s != null ? s.Zone : null)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync();
            
        return View("DocumentsList", new DocumentsListViewModel
        {
            Documents = documents,
            Title = "已過期文檔",
            SubTitle = "已過期需要更新的文檔",
            IconClass = "fa-calendar-times",
            StatusFilter = "已過期"
        });
    }
}
