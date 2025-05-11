using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using ETMF.Data;
using ETMF.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ETMF.Controllers
{
    public partial class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger<DocumentsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentsController(
            ApplicationDbContext context, 
            IWebHostEnvironment hostEnvironment, 
            ILogger<DocumentsController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: Documents
        [Authorize(Policy = "CanViewDocuments")]
        public async Task<IActionResult> Index()
        {
            var documents = await _context.Documents
                .Include(d => d.Artifact)
                    .ThenInclude(a => a != null ? a.Section : null)
                    .ThenInclude(s => s != null ? s.Zone : null)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
                
            return View(documents);
        }

        // GET: Documents/Details/5
        [Authorize(Policy = "CanViewDocuments")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Artifact)
                    .ThenInclude(a => a != null ? a.Section : null)
                    .ThenInclude(s => s != null ? s.Zone : null)
                .Include(d => d.Versions)
                .Include(d => d.Metadata)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (document == null)
            {
                return NotFound();
            }

            // 获取当前版本
            var currentVersion = document.Versions?
                .FirstOrDefault(v => v.Id == document.CurrentVersionId);
                
            ViewData["CurrentVersion"] = currentVersion;

            return View(document);
        }

        // GET: Documents/Create
        [Authorize(Policy = "CanUploadDocuments")]
        public async Task<IActionResult> Create(int? artifactId)
        {
            var artifacts = _context.Artifacts
                .Include(a => a.Section)
                .ThenInclude(s => s != null ? s.Zone : null)
                .OrderBy(a => a.Section != null && a.Section.Zone != null ? a.Section.Zone.Number : string.Empty)
                .ThenBy(a => a.Section != null ? a.Section.Number : string.Empty)
                .ThenBy(a => a.Number)
                .ToList();
                
            // 获取当前用户信息
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewBag.AuthorName = $"{user.FirstName} {user.LastName}".Trim();
            }
                
            if (artifactId.HasValue)
            {
                var artifact = artifacts.FirstOrDefault(a => a.Id == artifactId.Value);
                if (artifact != null)
                {
                    string artifactName = "";
                    if (artifact.Section != null && artifact.Section.Zone != null)
                    {
                        artifactName = $"{artifact.Section.Zone.Number}.{artifact.Section.Number}.{artifact.Number} - {artifact.Name}";
                    }
                    else
                    {
                        artifactName = $"{artifact.Number} - {artifact.Name}";
                    }
                    
                    ViewData["ArtifactId"] = artifact.Id;
                    ViewData["ArtifactName"] = artifactName;
                }
            }
            else
            {
                ViewData["ArtifactId"] = new SelectList(artifacts, "Id", "Name");
            }
            
            return View();
        }

        // POST: Documents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanUploadDocuments")]
        public async Task<IActionResult> Create([Bind("Title,Description,ArtifactId,TrialIdentifier")] Document document, IFormFile file)
        {
            try
            {
                // 添加一个日志标识符，方便跟踪
                string logId = Guid.NewGuid().ToString().Substring(0, 8);
                _logger.LogInformation($"[{logId}] 开始创建文档请求");
                
                // 获取当前用户
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // 自动设置作者为当前用户的姓名
                    document.Author = $"{user.FirstName} {user.LastName}".Trim();
                }
                else
                {
                    ModelState.AddModelError("Author", "无法获取当前用户信息");
                    await PrepareArtifactViewData(document.ArtifactId);
                    return View(document);
                }
                
                // 手动设置DocumentIdentifier，这样不需要在Bind中包含它
                document.DocumentIdentifier = Guid.NewGuid().ToString();
                
                if (ModelState.IsValid && file != null)
                {
                    // 檢查文件大小
                    if (file.Length == 0)
                    {
                        TempData["ErrorMessage"] = "上傳的文件為空，請選擇有效的文件";
                        ModelState.AddModelError("file", "上傳的文件為空，請選擇有效的文件");
                        _logger.LogWarning($"[{logId}] 上傳文件錯誤: 文件大小為0");
                        
                        // 準備視圖數據重新渲染
                        await PrepareArtifactViewData(document.ArtifactId);
                        return View(document);
                    }
                    
                    // 打印診斷信息
                    _logger.LogInformation($"[{logId}] 開始處理文件上傳: {file.FileName}, 大小: {file.Length} 字節");
                    
                    try
                    {
                        // 获取当前用户ID (如果已登录)
                        string userId = "1"; // 默认值
                        if (User.Identity != null && User.Identity.IsAuthenticated)
                        {
                            if (user != null)
                            {
                                userId = user.Id ?? string.Empty;
                            }
                        }
                        
                        // 設置創建信息
                        document.CreatedAt = DateTime.Now;
                        document.Status = "Draft"; // 初始狀態為草稿
                        document.CreatedById = 1;  // 使用固定的用户ID，避免GUID转换错误
                        document.Source = "Local"; // 本地上傳
                        document.IsArchived = false;
                        
                        _logger.LogInformation($"[{logId}] 保存文檔基本信息，DocumentIdentifier: {document.DocumentIdentifier}");
                        
                        // 保存文檔基本信息
                        _context.Add(document);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation($"[{logId}] 文檔已保存到數據庫，ID: {document.Id}");
                        
                        // 确保上传根目录存在
                        string uploadsRoot = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsRoot))
                        {
                            Directory.CreateDirectory(uploadsRoot);
                            _logger.LogInformation($"[{logId}] 創建上傳根目錄: {uploadsRoot}");
                        }
                        
                        // 确保文档目录存在
                        int documentId = document.Id;
                        string docFolder = Path.Combine(uploadsRoot, documentId.ToString());
                        if (!Directory.Exists(docFolder))
                        {
                            Directory.CreateDirectory(docFolder);
                            _logger.LogInformation($"[{logId}] 創建文檔目錄: {docFolder}");
                        }
                        
                        // 生成唯一文件名
                        int versionNumber = 1;
                        string originalFileName = Path.GetFileName(file.FileName);
                        string extension = Path.GetExtension(originalFileName);
                        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        string newFileName = $"v{versionNumber}_{timestamp}{extension}";
                        string fullPath = Path.Combine(docFolder, newFileName);
                        
                        _logger.LogInformation($"[{logId}] 准备保存文件到: {fullPath}");
                        
                        // 直接保存文件
                        using (var fileStream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        
                        // 验证文件是否成功保存
                        if (!System.IO.File.Exists(fullPath))
                        {
                            throw new IOException($"文件保存失败: {fullPath}");
                        }
                        
                        var fileInfo = new FileInfo(fullPath);
                        if (fileInfo.Length == 0)
                        {
                            throw new IOException("保存的文件大小为0");
                        }
                        
                        _logger.LogInformation($"[{logId}] 文件保存成功，文件大小: {fileInfo.Length} 字节");
                        
                        // 记录相对路径，用于后续访问
                        string relativePath = $"uploads/{documentId}/{newFileName}";
                        
                        // 创建版本记录
                        var version = new DocumentVersion
                        {
                            DocumentId = documentId,
                            VersionNumber = versionNumber,
                            FileName = originalFileName,
                            FilePath = relativePath,
                            FileSize = file.Length,
                            FileType = extension,
                            MimeType = file.ContentType ?? "application/octet-stream",
                            VersionComment = "Initial version",
                            UploadedById = 1, // 使用固定的用户ID，避免GUID转换错误
                            CreatedAt = DateTime.Now,
                            IsCurrent = true
                        };
                        
                        _logger.LogInformation($"[{logId}] 保存版本信息");
                        _context.DocumentVersions.Add(version);
                        await _context.SaveChangesAsync();
                        
                        // 更新文档当前版本
                        document.CurrentVersionId = version.Id;
                        _context.Update(document);
                        
                        // 创建基本元数据
                        await CreateBasicMetadata(document);
                        
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation($"[{logId}] 文档上传成功完成，文档ID: {document.Id}, 版本ID: {version.Id}");
                        
                        return RedirectToAction(nameof(Details), new { id = document.Id });
                    }
                    catch (Exception ex)
                    {
                        // 删除已创建的文档记录
                        _context.Documents.Remove(document);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogError(ex, $"[{logId}] 文件存储异常: {ex.Message}");
                        
                        TempData["ErrorMessage"] = $"文件保存失败: {ex.Message}";
                        ModelState.AddModelError("", $"文件保存失败: {ex.Message}");
                        
                        // 记录错误到日志文件
                        LogError(logId, $"文件保存异常", ex, file.FileName);
                    }
                }
                else
                {
                    if (file == null)
                    {
                        TempData["ErrorMessage"] = "請選擇要上傳的文件";
                        ModelState.AddModelError("file", "請選擇要上傳的文件");
                        _logger.LogWarning($"[{logId}] 上傳文件錯誤: 未選擇文件");
                    }
                    else
                    {
                        // 检查模型验证错误
                        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                        {
                            _logger.LogWarning($"[{logId}] 模型验证错误: {error.ErrorMessage}");
                        }
                    }
                }
                
                // 準備視圖數據用於重新渲染表單
                await PrepareArtifactViewData(document.ArtifactId);
                return View(document);
            }
            catch (Exception ex)
            {
                // 處理其他未預期的異常
                string logId = Guid.NewGuid().ToString().Substring(0, 8);
                _logger.LogError(ex, $"[{logId}] 意外异常: {ex.Message}");
                
                TempData["ErrorMessage"] = $"發生錯誤: {ex.Message}";
                ModelState.AddModelError("", $"發生錯誤: {ex.Message}");
                
                LogError(logId, "意外异常", ex, file?.FileName);
                
                await PrepareArtifactViewData(document.ArtifactId);
                return View(document);
            }
        }

        // GET: Documents/Edit/5
        [Authorize(Policy = "CanUploadDocuments")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            
            var artifacts = _context.Artifacts
                .Include(a => a.Section)
                .ThenInclude(s => s != null ? s.Zone : null)
                .OrderBy(a => a.Section != null && a.Section.Zone != null ? a.Section.Zone.Number : string.Empty)
                .ThenBy(a => a.Section != null ? a.Section.Number : string.Empty)
                .ThenBy(a => a.Number)
                .ToList();
                
            ViewData["ArtifactId"] = new SelectList(artifacts, "Id", "Name", document.ArtifactId);
            return View(document);
        }

        // POST: Documents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanUploadDocuments")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,ArtifactId,Author,Status,TrialIdentifier")] Document document)
        {
            if (id != document.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 获取原始文档以保留未修改的字段
                    var originalDocument = await _context.Documents.AsNoTracking()
                        .FirstOrDefaultAsync(d => d.Id == id);
                        
                    if (originalDocument != null)
                    {
                        document.DocumentIdentifier = originalDocument.DocumentIdentifier;
                        document.CreatedById = originalDocument.CreatedById;
                        document.CreatedAt = originalDocument.CreatedAt;
                        document.CurrentVersionId = originalDocument.CurrentVersionId;
                        document.IsArchived = originalDocument.IsArchived;
                        document.ArchivedAt = originalDocument.ArchivedAt;
                        document.Source = originalDocument.Source ?? string.Empty;
                        document.UpdatedAt = DateTime.Now;
                        
                        _context.Update(document);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = document.Id });
            }
            
            var artifacts = _context.Artifacts
                .Include(a => a.Section)
                .ThenInclude(s => s != null ? s.Zone : null)
                .OrderBy(a => a.Section != null && a.Section.Zone != null ? a.Section.Zone.Number : string.Empty)
                .ThenBy(a => a.Section != null ? a.Section.Number : string.Empty)
                .ThenBy(a => a.Number)
                .ToList();
                
            ViewData["ArtifactId"] = new SelectList(artifacts, "Id", "Name", document.ArtifactId);
            return View(document);
        }

        // GET: Documents/Delete/5
        [Authorize(Policy = "CanDeleteDocuments")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Artifact)
                .ThenInclude(a => a != null ? a.Section : null)
                .ThenInclude(s => s != null ? s.Zone : null)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanDeleteDocuments")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Documents
                .Include(d => d.Versions)
                .Include(d => d.Metadata)
                .FirstOrDefaultAsync(d => d.Id == id);
                
            if (document == null)
            {
                return NotFound();
            }
            
            // 删除文件
            if (document.Versions != null)
            {
                foreach (var version in document.Versions)
                {
                    if (version != null && !string.IsNullOrEmpty(version.FilePath) && System.IO.File.Exists(version.FilePath))
                    {
                        System.IO.File.Delete(version.FilePath);
                    }
                }
                
                // 删除版本
                _context.DocumentVersions.RemoveRange(document.Versions);
            }
            
            // 删除元数据
            if (document.Metadata != null)
            {
                _context.Metadata.RemoveRange(document.Metadata);
            }
            
            // 删除文档
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }
        
        // GET: Documents/UploadNewVersion/5
        [Authorize(Policy = "CanUploadDocuments")]
        public async Task<IActionResult> UploadNewVersion(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Artifact)
                .FirstOrDefaultAsync(d => d.Id == id);
                
            if (document == null)
            {
                return NotFound();
            }
            
            ViewData["Document"] = document;
            return View();
        }
        
        // POST: Documents/UploadNewVersion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanUploadDocuments")]
        public async Task<IActionResult> UploadNewVersion(int id, string versionComment, IFormFile file)
        {
            var document = await _context.Documents
                .Include(d => d.Versions)
                .FirstOrDefaultAsync(d => d.Id == id);
                
            if (document == null || file == null)
            {
                return NotFound();
            }
            
            // 创建新版本
            var version = await CreateDocumentVersion(document.Id, file, versionComment);
            
            // 更新文档当前版本
            document.CurrentVersionId = version.Id;
            document.UpdatedAt = DateTime.Now;
            _context.Update(document);
            
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Details), new { id = document.Id });
        }
        
        // GET: Documents/Download/5
        [Authorize(Policy = "CanViewDocuments")]
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Versions)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (document == null)
            {
                return NotFound();
            }
            
            // 获取当前版本
            var currentVersion = document.Versions?
                .FirstOrDefault(v => v.Id == document.CurrentVersionId);
                
            if (currentVersion == null)
            {
                TempData["ErrorMessage"] = "未找到文档文件";
                return RedirectToAction(nameof(Details), new { id = document.Id });
            }
            
            // 使用相对路径找到文件
            string filePath = Path.Combine(_hostEnvironment.WebRootPath, currentVersion.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            
            // 检查文件是否存在
            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "文件不存在或已被删除";
                return RedirectToAction(nameof(Details), new { id = document.Id });
            }
            
            // 确定文件的MIME类型
            string contentType = currentVersion.MimeType;
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "application/octet-stream";
            }
            
            // 返回文件
            var fileName = currentVersion.FileName;
            return PhysicalFile(filePath, contentType, fileName);
        }
        
        // GET: Documents/Archive/5
        [Authorize(Policy = "CanArchiveDocuments")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            
            document.IsArchived = true;
            document.ArchivedAt = DateTime.Now;
            document.UpdatedAt = DateTime.Now;
            
            _context.Update(document);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Details), new { id = document.Id });
        }
        
        // GET: Documents/Unarchive/5
        [Authorize(Policy = "CanArchiveDocuments")]
        public async Task<IActionResult> Unarchive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            
            document.IsArchived = false;
            document.ArchivedAt = null;
            document.UpdatedAt = DateTime.Now;
            
            _context.Update(document);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Details), new { id = document.Id });
        }
        
        // GET: Documents/ByArtifact/5
        [Authorize(Policy = "CanViewDocuments")]
        public async Task<IActionResult> ByArtifact(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artifact = await _context.Artifacts
                .Include(a => a.Section)
                .ThenInclude(s => s != null ? s.Zone : null)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == id);
                
            if (artifact == null)
            {
                return NotFound();
            }
            
            // 加载文档的附加信息
            if (artifact.Documents != null)
            {
                foreach (var doc in artifact.Documents)
                {
                    if (doc != null)
                    {
                        await _context.Entry(doc)
                            .Reference(d => d.Artifact)
                            .LoadAsync();
                            
                        await _context.Entry(doc)
                            .Collection(d => d.Versions)
                            .LoadAsync();
                    }
                }
            }
            
            ViewData["Artifact"] = artifact;
            return View(artifact.Documents?.ToList() ?? new List<Document>());
        }

        private async Task<DocumentVersion> CreateDocumentVersion(int documentId, IFormFile file, string versionComment)
        {
            try
            {
                // 添加logId变量以用于日志记录
                string logId = Guid.NewGuid().ToString().Substring(0, 8);
                
                // 获取文档的所有已有版本
                var existingVersions = await _context.DocumentVersions
                    .Where(v => v.DocumentId == documentId)
                    .ToListAsync();
                    
                // 确定新版本号
                int versionNumber = 1;
                if (existingVersions.Any())
                {
                    versionNumber = existingVersions.Max(v => v.VersionNumber) + 1;
                }
                
                // 确保uploads目录存在
                string uploadsRoot = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsRoot))
                {
                    Directory.CreateDirectory(uploadsRoot);
                }
                
                _logger.LogInformation($"WebRootPath: {_hostEnvironment.WebRootPath}");
                _logger.LogInformation($"Uploads Root: {uploadsRoot}");
                
                // 确保文档目录存在
                string docFolder = Path.Combine(uploadsRoot, documentId.ToString());
                if (!Directory.Exists(docFolder))
                {
                    Directory.CreateDirectory(docFolder);
                }
                
                _logger.LogInformation($"Document Folder: {docFolder}");
                
                // 生成唯一文件名
                string originalFileName = Path.GetFileName(file.FileName);
                string extension = Path.GetExtension(originalFileName);
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string newFileName = $"v{versionNumber}_{timestamp}{extension}";
                string fullPath = Path.Combine(docFolder, newFileName);
                
                _logger.LogInformation($"[{logId}] 准备保存文件到: {fullPath}");
                
                // 直接保存文件
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                
                // 验证文件是否成功保存
                if (!System.IO.File.Exists(fullPath))
                {
                    throw new IOException($"文件保存失败: {fullPath}");
                }
                
                var fileInfo = new FileInfo(fullPath);
                if (fileInfo.Length == 0)
                {
                    throw new IOException("保存的文件大小为0");
                }
                
                _logger.LogInformation($"[{logId}] 文件保存成功，大小: {fileInfo.Length} 字节");
                
                // 记录相对路径，用于后续访问
                string relativePath = $"uploads/{documentId}/{newFileName}";
                
                // 创建版本记录
                var version = new DocumentVersion
                {
                    DocumentId = documentId,
                    VersionNumber = versionNumber,
                    FileName = originalFileName,
                    FilePath = relativePath,
                    FileSize = file.Length,
                    FileType = extension,
                    MimeType = file.ContentType ?? "application/octet-stream",
                    VersionComment = versionComment ?? "New version",
                    UploadedById = 1, // 使用固定的用户ID，避免GUID转换错误
                    CreatedAt = DateTime.Now,
                    IsCurrent = true
                };
                
                // 将其他版本标记为非当前版本
                foreach (var oldVersion in existingVersions)
                {
                    oldVersion.IsCurrent = false;
                    _context.Update(oldVersion);
                }
                
                _context.Add(version);
                await _context.SaveChangesAsync();
                
                return version;
            }
            catch (Exception ex)
            {
                // 记录错误
                _logger.LogError(ex, "文件上传错误");
                
                // 写入日志文件
                string logPath = Path.Combine(_hostEnvironment.ContentRootPath, "logs");
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
                
                using (StreamWriter writer = new StreamWriter(Path.Combine(logPath, "upload_errors.log"), true))
                {
                    writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 文件上传错误: {ex.Message}");
                    writer.WriteLine($"文档ID: {documentId}, 文件名: {file.FileName}");
                    writer.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                    writer.WriteLine(new string('-', 80));
                }
                
                throw; // 重新抛出异常
            }
        }
        
        private async Task CreateBasicMetadata(Document document)
        {
            if (document == null)
            {
                return;
            }
            
            // 创建基本元数据
            var metadataList = new List<Metadata>
            {
                new Metadata
                {
                    DocumentId = document.Id,
                    Name = "Title",
                    Value = document.Title ?? string.Empty,
                    DataType = "Text",
                    IsSystemGenerated = true,
                    IsSearchable = true,
                    IsDisplayed = true,
                    CreatedAt = DateTime.Now
                },
                new Metadata
                {
                    DocumentId = document.Id,
                    Name = "Author",
                    Value = document.Author ?? string.Empty,
                    DataType = "Text",
                    IsSystemGenerated = true,
                    IsSearchable = true,
                    IsDisplayed = true,
                    CreatedAt = DateTime.Now
                },
                new Metadata
                {
                    DocumentId = document.Id,
                    Name = "CreationDate",
                    Value = document.CreatedAt.ToString("yyyy-MM-dd"),
                    DataType = "Date",
                    IsSystemGenerated = true,
                    IsSearchable = true,
                    IsDisplayed = true,
                    CreatedAt = DateTime.Now
                }
            };
            
            // 如果试验标识符不为空，添加相应元数据
            if (!string.IsNullOrEmpty(document.TrialIdentifier))
            {
                metadataList.Add(new Metadata
                {
                    DocumentId = document.Id,
                    Name = "TrialIdentifier",
                    Value = document.TrialIdentifier,
                    DataType = "Text",
                    IsSystemGenerated = true,
                    IsSearchable = true,
                    IsDisplayed = true,
                    CreatedAt = DateTime.Now
                });
            }
            
            _context.AddRange(metadataList);
            await _context.SaveChangesAsync();
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }

        // 輔助方法：準備文檔類型下拉選擇清單的數據
        private async Task PrepareArtifactViewData(int artifactId)
        {
            var artifacts = await _context.Artifacts
                .Include(a => a.Section)
                .ThenInclude(s => s != null ? s.Zone : null)
                .OrderBy(a => a.Section != null && a.Section.Zone != null ? a.Section.Zone.Number : string.Empty)
                .ThenBy(a => a.Section != null ? a.Section.Number : string.Empty)
                .ThenBy(a => a.Number)
                .ToListAsync();
                
            if (artifactId > 0)
            {
                var artifact = artifacts.FirstOrDefault(a => a.Id == artifactId);
                if (artifact != null)
                {
                    string artifactName = "";
                    if (artifact.Section != null && artifact.Section.Zone != null)
                    {
                        artifactName = $"{artifact.Section.Zone.Number}.{artifact.Section.Number}.{artifact.Number} - {artifact.Name}";
                    }
                    else
                    {
                        artifactName = $"{artifact.Number} - {artifact.Name}";
                    }
                    
                    ViewData["ArtifactId"] = artifact.Id;
                    ViewData["ArtifactName"] = artifactName;
                }
            }
            else
            {
                ViewData["ArtifactId"] = new SelectList(artifacts, "Id", "Name", artifactId);
            }
        }

        // 辅助方法：记录错误到日志文件
        private void LogError(string logId, string errorType, Exception ex, string? fileName)
        {
            try
            {
                string logPath = Path.Combine(_hostEnvironment.ContentRootPath, "logs");
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
                
                string logFile = Path.Combine(logPath, "upload_errors.log");
                
                using (StreamWriter writer = new StreamWriter(logFile, true))
                {
                    writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][{logId}] {errorType}: {ex.Message}");
                    writer.WriteLine($"文件名: {fileName ?? "未提供"}");
                    writer.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                    
                    if (ex.InnerException != null)
                    {
                        writer.WriteLine($"内部异常: {ex.InnerException.Message}");
                        writer.WriteLine($"内部异常堆栈: {ex.InnerException.StackTrace}");
                    }
                    
                    writer.WriteLine(new string('-', 80));
                }
            }
            catch (Exception logEx)
            {
                // 如果日志记录本身失败，只能输出到控制台
                Console.WriteLine($"[{logId}] 无法写入日志文件: {logEx.Message}");
            }
        }

        // GET: Documents/SubmitForReview/5
        [Authorize(Policy = "CanUploadDocuments")]
        public async Task<IActionResult> SubmitForReview(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            
            // 只有草稿或被退回的文檔可以提交審核
            if (document.Status != "草稿" && document.Status != "已退回")
            {
                TempData["ErrorMessage"] = "只有草稿或被退回的文檔可以提交審核";
                return RedirectToAction(nameof(Details), new { id = document.Id });
            }
            
            document.Status = "待審核";
            document.UpdatedAt = DateTime.Now;
            
            _context.Update(document);
            await _context.SaveChangesAsync();
            
            // 添加審計日誌
            _logger.LogInformation($"文檔 {document.Id} ({document.Title}) 已提交審核");
            
            TempData["SuccessMessage"] = "文檔已成功提交審核";
            return RedirectToAction(nameof(Details), new { id = document.Id });
        }

        // GET: Documents/Approve/5
        [Authorize(Policy = "CanApproveDocuments")]
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            
            // 只有待審核的文檔可以批准
            if (document.Status != "待審核")
            {
                TempData["ErrorMessage"] = "只有待審核的文檔可以批准";
                return RedirectToAction(nameof(Details), new { id = document.Id });
            }
            
            document.Status = "已批准";
            document.UpdatedAt = DateTime.Now;
            
            _context.Update(document);
            await _context.SaveChangesAsync();
            
            // 添加審計日誌
            _logger.LogInformation($"文檔 {document.Id} ({document.Title}) 已批准");
            
            TempData["SuccessMessage"] = "文檔已成功批准";
            return RedirectToAction(nameof(Details), new { id = document.Id });
        }

        // GET: Documents/Reject/5
        [Authorize(Policy = "CanRejectDocuments")]
        public async Task<IActionResult> Reject(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            
            // 只有待審核的文檔可以退回
            if (document.Status != "待審核")
            {
                TempData["ErrorMessage"] = "只有待審核的文檔可以退回";
                return RedirectToAction(nameof(Details), new { id = document.Id });
            }
            
            document.Status = "已退回";
            document.UpdatedAt = DateTime.Now;
            
            _context.Update(document);
            await _context.SaveChangesAsync();
            
            // 添加審計日誌
            _logger.LogInformation($"文檔 {document.Id} ({document.Title}) 已退回");
            
            TempData["SuccessMessage"] = "文檔已退回，等待修改";
            return RedirectToAction(nameof(Details), new { id = document.Id });
        }

        // 在DocumentsController中添加搜索方法
        [Authorize(Policy = "CanViewDocuments")]
        public async Task<IActionResult> Search(string searchTerm, int? zoneId, int? sectionId, int? artifactId, string status, string dateRange)
        {
            // 构建基础查询
            var query = _context.Documents
                .Include(d => d.Artifact)
                    .ThenInclude(a => a != null ? a.Section : null)
                    .ThenInclude(s => s != null ? s.Zone : null)
                .Include(d => d.Metadata)
                .AsQueryable();
            
            // 应用各种筛选条件
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => d.Title.Contains(searchTerm) || 
                                        d.Description.Contains(searchTerm) ||
                                        d.Author.Contains(searchTerm) ||
                                        d.DocumentIdentifier.Contains(searchTerm) ||
                                        d.TrialIdentifier.Contains(searchTerm) ||
                                        d.Metadata.Any(m => m.Value.Contains(searchTerm)));
            }
            
            if (zoneId.HasValue)
            {
                query = query.Where(d => d.Artifact != null && d.Artifact.Section != null && d.Artifact.Section.ZoneId == zoneId);
            }
            
            // 其他筛选条件...
            
            var results = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
            
            // 获取所有区域用于下拉列表
            var zones = await _context.Zones.OrderBy(z => z.Number).ToListAsync();
            
            // 构建视图模型并返回结果
            ViewData["SearchTerm"] = searchTerm;
            ViewData["ZoneId"] = zoneId;
            ViewData["Zones"] = zones;
            return View(results);
        }

        // 在DocumentsController中添加启动工作流方法
        [Authorize(Policy = "CanUploadDocuments")]
        public async Task<IActionResult> StartWorkflow(int id, int workflowId)
        {
            var document = await _context.Documents.FindAsync(id);
            var workflow = await _context.Workflows
                .Include(w => w.Steps.OrderBy(s => s.Order))
                .FirstOrDefaultAsync(w => w.Id == workflowId);
            
            if (document == null || workflow == null || !workflow.Steps.Any())
            {
                return NotFound();
            }
            
            var documentWorkflow = new DocumentWorkflow
            {
                DocumentId = document.Id,
                WorkflowId = workflow.Id,
                CurrentStepId = workflow.Steps.First().Id,
                Status = "InProgress",
                StartedAt = DateTime.Now
            };
            
            _context.Add(documentWorkflow);
            await _context.SaveChangesAsync();
            
            // 更新文档状态
            document.Status = "InReview";
            _context.Update(document);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Details), new { id = document.Id });
        }

        // 在DocumentsController中添加导出方法
        [Authorize(Policy = "CanViewDocuments")]
        public async Task<IActionResult> ExportDocuments(string format = "csv")
        {
            var documents = await _context.Documents
                .Include(d => d.Artifact)
                    .ThenInclude(a => a != null ? a.Section : null)
                    .ThenInclude(s => s != null ? s.Zone : null)
                .ToListAsync();
                
            if (format.ToLower() == "csv")
            {
                return ExportToCsv(documents);
            }
            else if (format.ToLower() == "excel")
            {
                return ExportToExcel(documents);
            }
            else
            {
                return BadRequest("不支持的格式");
            }
        }

        private IActionResult ExportToCsv(List<Document> documents)
        {
            var builder = new StringBuilder();

            // 添加CSV标题行
            builder.AppendLine("ID,标题,描述,文档类型,状态,作者,创建日期");

            // 添加数据行
            foreach (var doc in documents)
            {
                string artifactName = "未指定";
                if (doc.Artifact != null)
                {
                    artifactName = doc.Artifact.Name;
                }

                // 构建CSV行并转义字段
                builder.AppendLine($"{doc.Id},\"{doc.Title.Replace("\"", "\"\"")}\",\"{doc.Description.Replace("\"", "\"\"")}\",\"{artifactName}\",{doc.Status},{doc.Author},{doc.CreatedAt:yyyy-MM-dd}");
            }

            // 返回CSV文件
            byte[] bytes = Encoding.UTF8.GetBytes(builder.ToString());
            return File(bytes, "text/csv", $"documents_{DateTime.Now:yyyyMMdd}.csv");
        }

        // 在DocumentsController中添加批量导入方法
        [HttpPost]
        [Authorize(Policy = "CanUploadDocuments")]
        public IActionResult ImportDocuments(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "请选择要导入的文件";
                return RedirectToAction(nameof(Index));
            }

            // 处理导入逻辑，根据文件格式解析数据并创建文档

            return RedirectToAction(nameof(Index));
        }
        
        // 批量上传文档API端点
        [HttpPost]
        [Route("api/documents/batchupload")]
        [Authorize(Policy = "CanUploadDocuments")]
        public async Task<IActionResult> BatchUpload([FromForm] List<IFormFile> files, [FromForm] int artifactId, [FromForm] string trialIdentifier)
        {
            if (files == null || !files.Any())
            {
                return BadRequest(new { success = false, message = "未提供文件" });
            }
            
            var results = new List<object>();
            var logId = Guid.NewGuid().ToString().Substring(0, 8);
            
            // 获取当前用户
            var user = await _userManager.GetUserAsync(User);
            var author = user != null ? $"{user.FirstName} {user.LastName}".Trim() : string.Empty;
            
            _logger.LogInformation($"[{logId}] 开始批量上传，文件数量: {files.Count}，文档类型ID: {artifactId}");
            
            // 确保上传根目录存在
            string uploadsRoot = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsRoot))
            {
                Directory.CreateDirectory(uploadsRoot);
                _logger.LogInformation($"[{logId}] 创建上传根目录: {uploadsRoot}");
            }
            
            foreach (var file in files)
            {
                try
                {
                    _logger.LogInformation($"[{logId}] 开始处理文件: {file.FileName}");
                    
                    // 创建文档对象
                    var document = new Document
                    {
                        Title = Path.GetFileNameWithoutExtension(file.FileName),
                        Description = string.Empty,
                        ArtifactId = artifactId,
                        Author = author,
                        TrialIdentifier = trialIdentifier,
                        DocumentIdentifier = Guid.NewGuid().ToString(),
                        CreatedAt = DateTime.Now,
                        Status = "草稿",
                        CreatedById = user != null ? 1 : 0,
                        Source = "BatchUpload",
                        IsArchived = false
                    };
                    
                    // 保存文档基本信息
                    _context.Add(document);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"[{logId}] 文档基本信息已保存，ID: {document.Id}");
                    
                    // 确保文档目录存在
                    int documentId = document.Id;
                    string docFolder = Path.Combine(uploadsRoot, documentId.ToString());
                    if (!Directory.Exists(docFolder))
                    {
                        Directory.CreateDirectory(docFolder);
                        _logger.LogInformation($"[{logId}] 创建文档目录: {docFolder}");
                    }
                    
                    // 生成唯一文件名
                    int versionNumber = 1;
                    string originalFileName = Path.GetFileName(file.FileName);
                    string extension = Path.GetExtension(originalFileName);
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string newFileName = $"v{versionNumber}_{timestamp}{extension}";
                    string fullPath = Path.Combine(docFolder, newFileName);
                    
                    _logger.LogInformation($"[{logId}] 准备保存文件到: {fullPath}");
                    
                    // 直接保存文件
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    
                    // 验证文件是否成功保存
                    if (!System.IO.File.Exists(fullPath))
                    {
                        throw new IOException($"文件保存失败: {fullPath}");
                    }
                    
                    var fileInfo = new FileInfo(fullPath);
                    if (fileInfo.Length == 0)
                    {
                        throw new IOException("保存的文件大小为0");
                    }
                    
                    _logger.LogInformation($"[{logId}] 文件保存成功，大小: {fileInfo.Length} 字节");
                    
                    // 记录相对路径，用于后续访问
                    string relativePath = $"uploads/{documentId}/{newFileName}";
                    
                    // 创建版本记录
                    var version = new DocumentVersion
                    {
                        DocumentId = documentId,
                        VersionNumber = versionNumber,
                        FileName = originalFileName,
                        FilePath = relativePath,
                        FileSize = file.Length,
                        FileType = extension,
                        MimeType = file.ContentType ?? "application/octet-stream",
                        VersionComment = "批量上传初始版本",
                        UploadedById = user != null ? 1 : 0,
                        CreatedAt = DateTime.Now,
                        IsCurrent = true
                    };
                    
                    _context.DocumentVersions.Add(version);
                    await _context.SaveChangesAsync();
                    
                    // 更新文档当前版本
                    document.CurrentVersionId = version.Id;
                    _context.Update(document);
                    
                    // 创建基本元数据
                    await CreateBasicMetadata(document);
                    
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"[{logId}] 文件 {file.FileName} 上传成功，文档ID: {document.Id}");
                    
                    // 记录成功结果
                    results.Add(new { 
                        success = true, 
                        documentId = document.Id, 
                        fileName = file.FileName,
                        message = "上传成功" 
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{logId}] 文件 {file.FileName} 上传失败: {ex.Message}");
                    
                    // 记录失败结果
                    results.Add(new { 
                        success = false, 
                        fileName = file.FileName,
                        message = $"上传失败: {ex.Message}" 
                    });
                }
            }
            
            return Ok(new { 
                success = results.Count(r => (bool)((dynamic)r).success) > 0,
                totalFiles = files.Count,
                successCount = results.Count(r => (bool)((dynamic)r).success),
                failedCount = results.Count(r => !(bool)((dynamic)r).success),
                results = results
            });
        }
    }
} 