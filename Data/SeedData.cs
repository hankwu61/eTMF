using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ETMF.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ETMF.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // 检查数据库是否存在，如果不存在则创建
                context.Database.EnsureCreated();

                // 创建初始用户和角色
                await CreateRolesAndUsers(serviceProvider);

                // 检查是否已经有Zone数据
                if (context.Zones.Any())
                {
                    return; // 数据库已经初始化过
                }

                // 初始化TMF参考模型结构
                InitializeTmfStructure(context);
            }
        }

        private static async Task CreateRolesAndUsers(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            // 创建角色
            string[] roleNames = { 
                UserRole.Admin, 
                UserRole.DocumentManager, 
                UserRole.DocumentReviewer, 
                UserRole.DocumentApprover, 
                UserRole.DocumentViewer 
            };
            
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInformation($"已創建角色: {roleName}");
                }
            }

            // 创建管理员用户
            var adminUser = new ApplicationUser
            {
                UserName = "admin@etmf.com",
                Email = "admin@etmf.com",
                FirstName = "系统",
                LastName = "管理员",
                EmailConfirmed = true,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            var adminExists = await userManager.FindByEmailAsync(adminUser.Email);
            if (adminExists != null)
            {
                // 刪除現有管理員用戶，重新創建
                logger.LogInformation($"重新創建管理員用戶: {adminUser.Email}");
                await userManager.DeleteAsync(adminExists);
            }
            
            // 創建新的管理員用戶
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                logger.LogInformation($"管理員用戶創建成功: {adminUser.Email}");
                await userManager.AddToRoleAsync(adminUser, UserRole.Admin);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    logger.LogError($"創建管理員用戶失敗: {error.Description}");
                }
            }

            // 创建文档管理员用户
            var docManagerUser = new ApplicationUser
            {
                UserName = "manager@etmf.com",
                Email = "manager@etmf.com",
                FirstName = "文档",
                LastName = "管理员",
                EmailConfirmed = true,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            var managerExists = await userManager.FindByEmailAsync(docManagerUser.Email);
            if (managerExists != null)
            {
                // 刪除現有用戶，重新創建
                await userManager.DeleteAsync(managerExists);
            }
            
            result = await userManager.CreateAsync(docManagerUser, "Manager@123");
            if (result.Succeeded)
            {
                logger.LogInformation($"文檔管理員用戶創建成功: {docManagerUser.Email}");
                await userManager.AddToRoleAsync(docManagerUser, UserRole.DocumentManager);
            }
            
            // 创建文档审核员用户
            var reviewerUser = new ApplicationUser
            {
                UserName = "reviewer@etmf.com",
                Email = "reviewer@etmf.com",
                FirstName = "文档",
                LastName = "审核员",
                EmailConfirmed = true,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            var reviewerExists = await userManager.FindByEmailAsync(reviewerUser.Email);
            if (reviewerExists != null)
            {
                await userManager.DeleteAsync(reviewerExists);
            }
            
            result = await userManager.CreateAsync(reviewerUser, "Reviewer@123");
            if (result.Succeeded)
            {
                logger.LogInformation($"審核員用戶創建成功: {reviewerUser.Email}");
                await userManager.AddToRoleAsync(reviewerUser, UserRole.DocumentReviewer);
            }
            
            // 创建文档批准人用户
            var approverUser = new ApplicationUser
            {
                UserName = "approver@etmf.com",
                Email = "approver@etmf.com",
                FirstName = "文档",
                LastName = "批准人",
                EmailConfirmed = true,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            var approverExists = await userManager.FindByEmailAsync(approverUser.Email);
            if (approverExists != null)
            {
                await userManager.DeleteAsync(approverExists);
            }
            
            result = await userManager.CreateAsync(approverUser, "Approver@123");
            if (result.Succeeded)
            {
                logger.LogInformation($"批准人用戶創建成功: {approverUser.Email}");
                await userManager.AddToRoleAsync(approverUser, UserRole.DocumentApprover);
            }

            // 创建普通用户
            var viewerUser = new ApplicationUser
            {
                UserName = "user@etmf.com",
                Email = "user@etmf.com",
                FirstName = "普通",
                LastName = "用户",
                EmailConfirmed = true,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            var viewerExists = await userManager.FindByEmailAsync(viewerUser.Email);
            if (viewerExists != null)
            {
                await userManager.DeleteAsync(viewerExists);
            }
            
            result = await userManager.CreateAsync(viewerUser, "User@123");
            if (result.Succeeded)
            {
                logger.LogInformation($"普通用戶創建成功: {viewerUser.Email}");
                await userManager.AddToRoleAsync(viewerUser, UserRole.DocumentViewer);
            }
            
            // 創建一個測試用戶，密碼簡單易記
            var testUser = new ApplicationUser
            {
                UserName = "test@etmf.com",
                Email = "test@etmf.com",
                FirstName = "測試",
                LastName = "用戶",
                EmailConfirmed = true,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            
            var testExists = await userManager.FindByEmailAsync(testUser.Email);
            if (testExists != null)
            {
                await userManager.DeleteAsync(testExists);
            }
            
            result = await userManager.CreateAsync(testUser, "Test123!");
            if (result.Succeeded)
            {
                logger.LogInformation($"測試用戶創建成功: {testUser.Email}");
                await userManager.AddToRoleAsync(testUser, UserRole.Admin);
            }
        }

        private static void InitializeTmfStructure(ApplicationDbContext context)
        {
            // Zone 1: 试验管理
            var zone1 = new Zone
            {
                Number = "01",
                Name = "试验管理",
                Description = "与临床试验的整体管理有关的文件",
                CreatedAt = DateTime.Now
            };
            context.Zones.Add(zone1);

            // Zone 2: 试验监管
            var zone2 = new Zone
            {
                Number = "02",
                Name = "试验监管",
                Description = "与监管机构文件和通信有关的文件",
                CreatedAt = DateTime.Now
            };
            context.Zones.Add(zone2);

            // Zone 3: 伦理委员会
            var zone3 = new Zone
            {
                Number = "03",
                Name = "伦理委员会",
                Description = "与伦理委员会、伦理审查委员会(IRB)或独立伦理委员会(IEC)相关的文件",
                CreatedAt = DateTime.Now
            };
            context.Zones.Add(zone3);

            // Zone 4: 研究中心
            var zone4 = new Zone
            {
                Number = "04",
                Name = "研究中心",
                Description = "与研究中心选择、设立和管理有关的文件",
                CreatedAt = DateTime.Now
            };
            context.Zones.Add(zone4);

            // Zone 5: 实验室
            var zone5 = new Zone
            {
                Number = "05",
                Name = "实验室",
                Description = "与实验室工作和分析相关的文件",
                CreatedAt = DateTime.Now
            };
            context.Zones.Add(zone5);

            context.SaveChanges();

            // Zone 1的子部分和文档类型
            // Section 1.1: 试验规划与审批
            var section1_1 = new Section
            {
                ZoneId = zone1.Id,
                Number = "01",
                Name = "试验规划与审批",
                Description = "与临床试验计划和批准相关的文件",
                CreatedAt = DateTime.Now
            };
            context.Sections.Add(section1_1);

            // Section 1.2: 试验管理
            var section1_2 = new Section
            {
                ZoneId = zone1.Id,
                Number = "02",
                Name = "试验管理",
                Description = "与临床试验执行和管理相关的文件",
                CreatedAt = DateTime.Now
            };
            context.Sections.Add(section1_2);

            context.SaveChanges();

            // 添加文档类型
            // Section 1.1的文档类型
            context.Artifacts.Add(new Artifact
            {
                SectionId = section1_1.Id,
                Number = "01",
                Name = "试验方案",
                Description = "临床试验方案",
                IsRequired = true,
                CreatedAt = DateTime.Now
            });

            context.Artifacts.Add(new Artifact
            {
                SectionId = section1_1.Id,
                Number = "02",
                Name = "试验方案修订",
                Description = "临床试验方案修订版本",
                IsRequired = true,
                CreatedAt = DateTime.Now
            });

            context.Artifacts.Add(new Artifact
            {
                SectionId = section1_1.Id,
                Number = "03",
                Name = "研究者手册",
                Description = "研究者手册和更新版本",
                IsRequired = true,
                CreatedAt = DateTime.Now
            });

            // Section 1.2的文档类型
            context.Artifacts.Add(new Artifact
            {
                SectionId = section1_2.Id,
                Number = "01",
                Name = "试验管理计划",
                Description = "临床试验管理计划文件",
                IsRequired = true,
                CreatedAt = DateTime.Now
            });

            context.Artifacts.Add(new Artifact
            {
                SectionId = section1_2.Id,
                Number = "02",
                Name = "监查计划",
                Description = "临床试验监查计划",
                IsRequired = true,
                CreatedAt = DateTime.Now
            });

            context.Artifacts.Add(new Artifact
            {
                SectionId = section1_2.Id,
                Number = "03",
                Name = "数据管理计划",
                Description = "临床试验数据管理计划",
                IsRequired = true,
                CreatedAt = DateTime.Now
            });

            // Zone 2的子部分和文档类型
            var section2_1 = new Section
            {
                ZoneId = zone2.Id,
                Number = "01",
                Name = "监管机构申请",
                Description = "提交给监管机构的申请文件",
                CreatedAt = DateTime.Now
            };
            context.Sections.Add(section2_1);

            context.SaveChanges();

            // Section 2.1的文档类型
            context.Artifacts.Add(new Artifact
            {
                SectionId = section2_1.Id,
                Number = "01",
                Name = "监管申请表",
                Description = "提交给NMPA或其他监管机构的申请表",
                IsRequired = true,
                CreatedAt = DateTime.Now
            });

            context.Artifacts.Add(new Artifact
            {
                SectionId = section2_1.Id,
                Number = "02",
                Name = "监管批准文件",
                Description = "监管机构的批准文件",
                IsRequired = true,
                CreatedAt = DateTime.Now
            });

            // 创建工作流定义
            var approvalWorkflow = new Workflow
            {
                Name = "文档审批流程",
                Description = "标准文档审批流程，需要审核和批准",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Workflows.Add(approvalWorkflow);
            context.SaveChanges();

            // 添加工作流步骤
            context.WorkflowSteps.Add(new WorkflowStep
            {
                WorkflowId = approvalWorkflow.Id,
                Name = "初审",
                Order = 1,
                ApproverRoleId = "DocumentManager",
                ActionType = "Review",
                CreatedAt = DateTime.Now
            });

            context.WorkflowSteps.Add(new WorkflowStep
            {
                WorkflowId = approvalWorkflow.Id,
                Name = "终审",
                Order = 2,
                ApproverRoleId = "Admin",
                ActionType = "Approve",
                CreatedAt = DateTime.Now
            });

            context.SaveChanges();
        }
    }
} 