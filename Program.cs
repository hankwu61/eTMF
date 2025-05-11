using Microsoft.EntityFrameworkCore;
using ETMF.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ETMF.Models;
using ETMF.Services;

var builder = WebApplication.CreateBuilder(args);

// 添加数据库上下文
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();

// 配置身份验证
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // 配置密码复杂度要求
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    // 配置锁定设置
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 配置Cookie设置
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// 配置认证策略
builder.Services.AddAuthorization(options =>
{
    // 基于角色的策略
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole(UserRole.Admin));
    options.AddPolicy("DocumentManager", policy => policy.RequireRole(UserRole.Admin, UserRole.DocumentManager));
    options.AddPolicy("DocumentReviewer", policy => policy.RequireRole(UserRole.Admin, UserRole.DocumentReviewer));
    options.AddPolicy("DocumentApprover", policy => policy.RequireRole(UserRole.Admin, UserRole.DocumentApprover));
    options.AddPolicy("DocumentViewer", policy => policy.RequireRole(UserRole.Admin, UserRole.DocumentManager, UserRole.DocumentReviewer, UserRole.DocumentApprover, UserRole.DocumentViewer));
    
    // 基于权限的策略
    options.AddPolicy("CanUploadDocuments", policy => 
        policy.RequireAssertion(context => 
            context.User.IsInRole(UserRole.Admin) || 
            context.User.IsInRole(UserRole.DocumentManager)));
            
    options.AddPolicy("CanReviewDocuments", policy => 
        policy.RequireAssertion(context => 
            context.User.IsInRole(UserRole.Admin) || 
            context.User.IsInRole(UserRole.DocumentReviewer)));
            
    options.AddPolicy("CanApproveDocuments", policy => 
        policy.RequireAssertion(context => 
            context.User.IsInRole(UserRole.Admin) || 
            context.User.IsInRole(UserRole.DocumentApprover)));
            
    options.AddPolicy("CanRejectDocuments", policy => 
        policy.RequireAssertion(context => 
            context.User.IsInRole(UserRole.Admin) || 
            context.User.IsInRole(UserRole.DocumentReviewer) || 
            context.User.IsInRole(UserRole.DocumentApprover)));
            
    options.AddPolicy("CanArchiveDocuments", policy => 
        policy.RequireAssertion(context => 
            context.User.IsInRole(UserRole.Admin) || 
            context.User.IsInRole(UserRole.DocumentManager)));
            
    options.AddPolicy("CanDeleteDocuments", policy => 
        policy.RequireRole(UserRole.Admin));
            
    options.AddPolicy("CanViewDocuments", policy => 
        policy.RequireAssertion(context => 
            context.User.IsInRole(UserRole.Admin) || 
            context.User.IsInRole(UserRole.DocumentManager) || 
            context.User.IsInRole(UserRole.DocumentReviewer) || 
            context.User.IsInRole(UserRole.DocumentApprover) || 
            context.User.IsInRole(UserRole.DocumentViewer)));
            
    options.AddPolicy("CanManageUsers", policy => 
        policy.RequireRole(UserRole.Admin));
        
    options.AddPolicy("CanManageRoles", policy => 
        policy.RequireRole(UserRole.Admin));
        
    options.AddPolicy("CanManageSystem", policy => 
        policy.RequireRole(UserRole.Admin));
});

// 注册提醒服务
builder.Services.AddHostedService<ReminderService>();

var app = builder.Build();

// 初始化种子数据
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        SeedData.Initialize(services).Wait();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "初始化数据库时出错。");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
