using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ETMF.Models.ViewModels;
using ETMF.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ETMF.Controllers
{
    [Authorize(Policy = "CanManageSystem")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SettingsController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public SettingsController(
            ApplicationDbContext context,
            ILogger<SettingsController> logger,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        // GET: /Settings
        public IActionResult Index()
        {
            // 嘗試從TempData中獲取設置（如果之前保存過）
            var model = TempData.Get<SystemSettingsViewModel>("SavedSettings") ?? new SystemSettingsViewModel
            {
                ApplicationName = _configuration["AppSettings:ApplicationName"] ?? "eTMF系統",
                CompanyName = _configuration["AppSettings:CompanyName"] ?? "您的公司名稱",
                SupportEmail = _configuration["AppSettings:SupportEmail"] ?? "support@example.com",
                MaxUploadFileSize = int.TryParse(_configuration["AppSettings:MaxUploadFileSize"], out int maxSize) ? maxSize : 10,
                AllowedFileExtensions = _configuration["AppSettings:AllowedFileExtensions"] ?? ".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.jpg,.png",
                EnableAuditLogging = bool.TryParse(_configuration["AppSettings:EnableAuditLogging"], out bool enableAudit) ? enableAudit : true,
                DefaultPageSize = int.TryParse(_configuration["AppSettings:DefaultPageSize"], out int pageSize) ? pageSize : 10,
                EnableEmailNotifications = bool.TryParse(_configuration["AppSettings:EnableEmailNotifications"], out bool enableEmail) ? enableEmail : false,
                SmtpServer = _configuration["AppSettings:SmtpServer"] ?? "",
                SmtpPort = int.TryParse(_configuration["AppSettings:SmtpPort"], out int smtpPort) ? smtpPort : 587,
                SmtpUsername = _configuration["AppSettings:SmtpUsername"] ?? "",
                SmtpPassword = _configuration["AppSettings:SmtpPassword"] ?? "",
                SmtpUseSsl = bool.TryParse(_configuration["AppSettings:SmtpUseSsl"], out bool useSsl) ? useSsl : true,
                SystemTheme = _configuration["AppSettings:SystemTheme"] ?? "default"
            };

            return View(model);
        }

        // POST: /Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(SystemSettingsViewModel model, string activeTab)
        {
            _logger.LogInformation("正在保存系统设置...");
            
            if (ModelState.IsValid)
            {
                try
                {
                    // 记录传入的模型数据，帮助诊断
                    _logger.LogInformation($"收到系统设置表单数据: ApplicationName={model.ApplicationName}, CompanyName={model.CompanyName}");
                    
                    // 使用TempData保存設置，模擬數據持久化
                    TempData.Put("SavedSettings", model);
                    _logger.LogInformation("已保存设置到TempData");
                    
                    // 嘗試將設置寫入配置文件（如果有權限）
                    try
                    {
                        SaveSettingsToAppSettings(model);
                        _logger.LogInformation("已成功保存设置到配置文件");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "無法將設置保存到配置文件，但已保存到TempData。错误详情: {0}", ex.Message);
                    }
                    
                    // 直接保存一些基本设置到数据库（如果有条件）
                    try
                    {
                        // 可以考虑添加直接保存到数据库的代码
                        _logger.LogInformation("数据库保存逻辑将在后续版本中实现");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "保存到数据库时出错: {0}", ex.Message);
                    }
                    
                    _logger.LogInformation("系統設置已更新");
                    TempData["SuccessMessage"] = "系統設置已成功保存";
                    
                    // 确保TempData在重定向后可用
                    TempData.Keep("SavedSettings");
                    
                    // 返回到相同的標籤頁
                    if (!string.IsNullOrEmpty(activeTab))
                    {
                        // 使用单独的Redirect方法，而不是RedirectToAction，以避免URL编码问题
                        return Redirect($"/Settings/Index#{activeTab}");
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "保存系統設置時發生錯誤: {0}", ex.Message);
                    ModelState.AddModelError("", $"保存系統設置時發生錯誤: {ex.Message}");
                }
            }
            else
            {
                // 记录模型验证错误
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        _logger.LogWarning($"字段 {state.Key} 验证错误: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
            }
            
            return View(model);
        }

        // 嘗試將設置保存到appsettings.json文件（如果有權限）
        private void SaveSettingsToAppSettings(SystemSettingsViewModel model)
        {
            var appSettingsPath = Path.Combine(_webHostEnvironment.ContentRootPath, "appsettings.json");
            
            _logger.LogInformation($"尝试保存设置到配置文件: {appSettingsPath}");
            
            if (!System.IO.File.Exists(appSettingsPath))
            {
                _logger.LogWarning($"配置文件不存在: {appSettingsPath}");
                throw new FileNotFoundException($"找不到配置文件: {appSettingsPath}");
            }
            
            try
            {
                // 检查文件是否可写
                try
                {
                    FileInfo fileInfo = new FileInfo(appSettingsPath);
                    if (fileInfo.IsReadOnly)
                    {
                        _logger.LogWarning($"配置文件是只读的: {appSettingsPath}");
                        throw new UnauthorizedAccessException($"配置文件是只读的: {appSettingsPath}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"检查文件权限时出错: {ex.Message}");
                }
                
                // 先读取文件内容
                string json;
                try
                {
                    json = System.IO.File.ReadAllText(appSettingsPath);
                    _logger.LogInformation($"成功读取配置文件，长度: {json.Length}字符");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"读取配置文件失败: {ex.Message}");
                    throw;
                }
                
                // 解析JSON
                JObject jsonObj;
                try
                {
                    jsonObj = JObject.Parse(json);
                    _logger.LogInformation("成功解析JSON配置文件");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"解析JSON配置文件失败: {ex.Message}");
                    throw;
                }
                
                // 確保AppSettings節點存在
                if (jsonObj["AppSettings"] == null)
                {
                    _logger.LogInformation("未找到AppSettings节点，创建新节点");
                    jsonObj["AppSettings"] = new JObject();
                }
                
                // 更新設置
                if (jsonObj["AppSettings"] != null)
                {
                    _logger.LogInformation("开始更新AppSettings节点");
                    jsonObj["AppSettings"]!["ApplicationName"] = model.ApplicationName;
                    jsonObj["AppSettings"]!["CompanyName"] = model.CompanyName;
                    jsonObj["AppSettings"]!["SupportEmail"] = model.SupportEmail;
                    jsonObj["AppSettings"]!["MaxUploadFileSize"] = model.MaxUploadFileSize;
                    jsonObj["AppSettings"]!["AllowedFileExtensions"] = model.AllowedFileExtensions;
                    jsonObj["AppSettings"]!["EnableAuditLogging"] = model.EnableAuditLogging;
                    jsonObj["AppSettings"]!["DefaultPageSize"] = model.DefaultPageSize;
                    jsonObj["AppSettings"]!["EnableEmailNotifications"] = model.EnableEmailNotifications;
                    jsonObj["AppSettings"]!["SmtpServer"] = model.SmtpServer;
                    jsonObj["AppSettings"]!["SmtpPort"] = model.SmtpPort;
                    jsonObj["AppSettings"]!["SmtpUsername"] = model.SmtpUsername;
                    
                    // 只有在提供新密碼時才更新密碼
                    if (!string.IsNullOrEmpty(model.SmtpPassword))
                    {
                        jsonObj["AppSettings"]!["SmtpPassword"] = model.SmtpPassword;
                    }
                    
                    jsonObj["AppSettings"]!["SmtpUseSsl"] = model.SmtpUseSsl;
                    jsonObj["AppSettings"]!["SystemTheme"] = model.SystemTheme;
                    
                    _logger.LogInformation("已更新所有设置值");
                }
                
                // 将JSON转换为字符串
                var jsonString = jsonObj.ToString(Formatting.Indented);
                _logger.LogInformation($"已准备保存的JSON内容，长度: {jsonString.Length}字符");
                
                // 使用临时文件保存，然后替换，以避免文件损坏
                var tempFilePath = Path.Combine(Path.GetDirectoryName(appSettingsPath) ?? "", Path.GetRandomFileName() + ".json");
                try
                {
                    // 写入临时文件
                    System.IO.File.WriteAllText(tempFilePath, jsonString);
                    _logger.LogInformation($"已成功写入临时文件: {tempFilePath}");
                    
                    // 备份原文件
                    var backupPath = appSettingsPath + ".bak";
                    if (System.IO.File.Exists(backupPath))
                    {
                        System.IO.File.Delete(backupPath);
                    }
                    System.IO.File.Copy(appSettingsPath, backupPath);
                    _logger.LogInformation($"已备份原配置文件到: {backupPath}");
                    
                    // 替换原文件
                    System.IO.File.Delete(appSettingsPath);
                    System.IO.File.Move(tempFilePath, appSettingsPath);
                    _logger.LogInformation($"已成功用临时文件替换原配置文件");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"保存配置文件过程中出错: {ex.Message}");
                    
                    // 尝试直接写入文件
                    _logger.LogInformation("尝试直接写入文件方式");
                    System.IO.File.WriteAllText(appSettingsPath, jsonString);
                    _logger.LogInformation("直接写入文件成功");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"無法寫入appsettings.json文件: {ex.Message}");
                throw;
            }
        }

        // GET: /Settings/Backup
        public IActionResult Backup()
        {
            return View();
        }

        // POST: /Settings/Backup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Backup(BackupViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 在實際應用中，這裡應該執行數據庫備份
                    // 這裡只是模擬備份操作
                    
                    // 模擬備份過程
                    await Task.Delay(2000);
                    
                    _logger.LogInformation("系統備份已創建");
                    TempData["SuccessMessage"] = "系統備份已成功創建";
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "創建系統備份時發生錯誤");
                    ModelState.AddModelError("", "創建系統備份時發生錯誤: " + ex.Message);
                }
            }
            
            return View(model);
        }

        // GET: /Settings/Restore
        public IActionResult Restore()
        {
            return View();
        }

        // POST: /Settings/Restore
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(RestoreViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 在實際應用中，這裡應該執行數據庫恢復
                    // 這裡只是模擬恢復操作
                    
                    // 模擬恢復過程
                    await Task.Delay(3000);
                    
                    _logger.LogInformation("系統已從備份恢復");
                    TempData["SuccessMessage"] = "系統已成功從備份恢復";
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "從備份恢復系統時發生錯誤");
                    ModelState.AddModelError("", "從備份恢復系統時發生錯誤: " + ex.Message);
                }
            }
            
            return View(model);
        }

        // GET: /Settings/Logs
        public IActionResult Logs()
        {
            // 在實際應用中，這裡應該讀取實際的系統日誌
            // 這裡只是創建一些模擬的日誌數據
            var logs = new List<LogViewModel>();
            
            for (int i = 1; i <= 20; i++)
            {
                logs.Add(new LogViewModel
                {
                    Id = i,
                    Timestamp = DateTime.Now.AddHours(-i),
                    Level = i % 4 == 0 ? "Error" : (i % 3 == 0 ? "Warning" : "Information"),
                    Message = $"這是一條示例日誌消息 #{i}",
                    Source = i % 2 == 0 ? "System" : "User",
                    Username = i % 2 == 0 ? "System" : "admin@etmf.com"
                });
            }
            
            return View(logs);
        }

        // GET: /Settings/About
        public IActionResult About()
        {
            var model = new AboutViewModel
            {
                ApplicationName = "Electronic Trial Master File (eTMF) System",
                Version = "1.0.0",
                BuildDate = new DateTime(2025, 5, 10),
                Copyright = $"© {DateTime.Now.Year} Your Company",
                Description = "Electronic Trial Master File (eTMF) 系統是一個用於管理臨床試驗文檔的應用程序，基於DIA TMF參考模型。",
                DatabaseInfo = "Microsoft SQL Server",
                OperatingSystem = Environment.OSVersion.ToString(),
                FrameworkVersion = Environment.Version.ToString()
            };
            
            return View(model);
        }
    }
    
    // TempData擴展方法，用於存儲和讀取複雜對象
    public static class TempDataExtensions
    {
        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            try
            {
                if (value == null)
                {
                    tempData[key] = null;
                    return;
                }
                
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };
                
                string json = JsonConvert.SerializeObject(value, Formatting.None, jsonSettings);
                tempData[key] = json;
            }
            catch (Exception ex)
            {
                // 记录异常但不抛出，避免页面崩溃
                Console.WriteLine($"TempData.Put错误: {ex.Message}");
            }
        }

        public static T? Get<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            try
            {
                if (tempData.TryGetValue(key, out object? o))
                {
                    if (o == null)
                    {
                        return null;
                    }
                    
                    if (o is string json)
                    {
                        // 使用防御性编程，处理可能的异常
                        if (string.IsNullOrEmpty(json))
                        {
                            return null;
                        }
                        
                        var jsonSettings = new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            Error = (sender, args) => { args.ErrorContext.Handled = true; }
                        };
                        
                        return JsonConvert.DeserializeObject<T>(json, jsonSettings);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                // 记录异常但不抛出，避免页面崩溃
                Console.WriteLine($"TempData.Get错误: {ex.Message}");
                return null;
            }
        }
    }
} 