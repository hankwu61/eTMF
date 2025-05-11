using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ETMF.Data;
using ETMF.Models;

namespace ETMF.Services
{
    // 创建后台服务来检查和发送提醒
    public class ReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReminderService> _logger;
        
        public ReminderService(IServiceScopeFactory scopeFactory, ILogger<ReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("提醒服务已启动");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("开始处理提醒");
                    await ProcessReminders();
                    _logger.LogInformation("提醒处理完成，等待下一次检查");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理提醒时发生错误");
                }
                
                // 每天检查一次
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
        
        private async Task ProcessReminders()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var today = DateTime.Today;
            
            // 获取所有激活的提醒，然后在内存中进行日期比较
            var reminders = await context.DocumentReminders
                .Include(r => r.Document)
                .Where(r => r.IsActive)
                .ToListAsync();
                
            // 在内存中过滤需要发送提醒的文档
            var remindersToSend = reminders
                .Where(r => (r.ExpiryDate.Date - today).Days <= r.ReminderDaysBeforeExpiry)
                .ToList();
                
            _logger.LogInformation($"找到 {remindersToSend.Count} 个需要发送提醒的文档");
            
            foreach (var reminder in remindersToSend)
            {
                await SendReminder(reminder);
            }
        }
        
        private async Task SendReminder(DocumentReminder reminder)
        {
            // 实现提醒发送逻辑
            _logger.LogInformation($"发送提醒: 文档 ID {reminder.DocumentId}, 将于 {reminder.ExpiryDate:yyyy-MM-dd} 到期");
            
            // 这里可以实现邮件发送逻辑
            // 例如:
            // await _emailService.SendEmailAsync(reminder.NotificationRecipients, "文档到期提醒", 
            //     $"文档 {reminder.Document.Title} 将于 {reminder.ExpiryDate:yyyy-MM-dd} 到期，请及时处理。");
            
            // 示例：更新最后发送时间
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var reminderToUpdate = await context.DocumentReminders.FindAsync(reminder.Id);
            if (reminderToUpdate != null)
            {
                reminderToUpdate.LastSentAt = DateTime.Now;
                context.Update(reminderToUpdate);
                await context.SaveChangesAsync();
            }
        }
    }
} 