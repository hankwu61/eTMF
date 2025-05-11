using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using ETMF.Models;
using ETMF.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;

namespace ETMF.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        
        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }
        
        public IActionResult AccessDenied()
        {
            return View();
        }
        
        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation($"嘗試登入用戶: {model.Email}");
                
                // 先查找用戶是否存在
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    _logger.LogWarning($"登入失敗: 用戶不存在 - {model.Email}");
                    ModelState.AddModelError(string.Empty, "用户名或密码不正确");
                    return View(model);
                }
                
                // 檢查用戶是否啟用
                if (!user.IsActive)
                {
                    _logger.LogWarning($"登入失敗: 用戶已停用 - {model.Email}");
                    ModelState.AddModelError(string.Empty, "此账户已被禁用，请联系管理员");
                    return View(model);
                }
                
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation($"用戶登入成功: {model.Email}");
                    
                    // 更新最後登入時間
                    user.LastLoginAt = DateTime.Now;
                    await _userManager.UpdateAsync(user);
                    
                    return RedirectToAction("Index", "Home");
                }
                
                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"登入失敗: 用戶已鎖定 - {model.Email}");
                    ModelState.AddModelError(string.Empty, "账户已锁定，请稍后再试");
                }
                else
                {
                    _logger.LogWarning($"登入失敗: 密碼錯誤 - {model.Email}");
                    ModelState.AddModelError(string.Empty, "用户名或密码不正确");
                }
            }
            
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("用戶已登出");
            return RedirectToAction("Index", "Home");
        }
        
        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            
            return View(model);
        }
        
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            
            var model = new ProfileViewModel
            {
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty
            };
            
            return View(model);
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            
            if (user.Email != model.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
                
                user.UserName = model.Email;
            }
            
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
            
            TempData["StatusMessage"] = "您的個人資料已更新";
            return RedirectToAction(nameof(Profile));
        }
        
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
            
            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "您的密碼已變更";
            return RedirectToAction(nameof(Profile));
        }
    }
} 