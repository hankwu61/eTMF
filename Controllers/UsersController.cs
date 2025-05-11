using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ETMF.Models;
using ETMF.Models.ViewModels;

namespace ETMF.Controllers
{
    [Authorize(Policy = "CanManageUsers")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .ToListAsync();
                
            var userViewModels = new List<UserViewModel>();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    IsActive = user.IsActive,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                });
            }
            
            return View(userViewModels);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            
            var userViewModel = new UserViewModel
            {
                Id = user.Id ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                IsActive = user.IsActive,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return View(userViewModel);
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            ViewBag.Roles = roles;
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    IsActive = model.IsActive,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };
                
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }
                    
                    TempData["SuccessMessage"] = "用户创建成功";
                    return RedirectToAction(nameof(Index));
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            ViewBag.Roles = roles;
            return View(model);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);
            
            var model = new EditUserViewModel
            {
                Id = user.Id ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                IsActive = user.IsActive,
                SelectedRoles = userRoles.ToList()
            };
            
            ViewBag.Roles = roles;
            return View(model);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id ?? string.Empty);
                if (user == null)
                {
                    return NotFound();
                }
                
                user.Email = model.Email;
                user.UserName = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.IsActive = model.IsActive;
                
                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    // 更新角色
                    var userRoles = await _userManager.GetRolesAsync(user);
                    
                    // 移除所有现有角色
                    await _userManager.RemoveFromRolesAsync(user, userRoles);
                    
                    // 添加选定的角色
                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }
                    
                    TempData["SuccessMessage"] = "用户信息更新成功";
                    return RedirectToAction(nameof(Index));
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            ViewBag.Roles = roles;
            return View(model);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            
            var userViewModel = new UserViewModel
            {
                Id = user.Id ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                IsActive = user.IsActive,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return View(userViewModel);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            // 检查是否是当前登录用户
            if (User.Identity != null && User.Identity.Name == user.UserName)
            {
                TempData["ErrorMessage"] = "不能删除当前登录的用户";
                return RedirectToAction(nameof(Index));
            }
            
            // 检查是否是系统管理员
            if (await _userManager.IsInRoleAsync(user, UserRole.Admin) && 
                user.UserName == "admin@etmf.com")
            {
                TempData["ErrorMessage"] = "不能删除系统管理员账户";
                return RedirectToAction(nameof(Index));
            }
            
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "用户删除成功";
                return RedirectToAction(nameof(Index));
            }
            
            foreach (var error in result.Errors)
            {
                TempData["ErrorMessage"] = error.Description;
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        // GET: Users/ResetPassword/5
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            var model = new ResetPasswordViewModel
            {
                Id = user.Id ?? string.Empty,
                Email = user.Email ?? string.Empty
            };
            
            return View(model);
        }
        
        // POST: Users/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = await _userManager.FindByIdAsync(model.Id ?? string.Empty);
            if (user == null)
            {
                return NotFound();
            }
            
            // 生成密码重置令牌
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // 重置密码
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "密码重置成功";
                return RedirectToAction(nameof(Index));
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            
            return View(model);
        }
    }
} 