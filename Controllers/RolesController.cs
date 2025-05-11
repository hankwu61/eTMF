using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using ETMF.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using ETMF.Models.ViewModels;
using System;

namespace ETMF.Controllers
{
    [Authorize(Policy = "CanManageRoles")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Roles
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        // GET: /Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.Name);
                if (roleExists)
                {
                    ModelState.AddModelError("", "角色名稱已存在");
                    return View(model);
                }

                var result = await _roleManager.CreateAsync(new IdentityRole(model.Name));
                if (result.Succeeded)
                {
                    _logger.LogInformation($"創建角色成功: {model.Name}");
                    TempData["SuccessMessage"] = "角色創建成功";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // GET: /Roles/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var model = new RoleViewModel
            {
                Id = role.Id ?? string.Empty,
                Name = role.Name ?? string.Empty
            };

            return View(model);
        }

        // POST: /Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, RoleViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                role.Name = model.Name ?? string.Empty;
                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"更新角色成功: {model.Name}");
                    TempData["SuccessMessage"] = "角色更新成功";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // GET: /Roles/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // POST: /Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // 檢查是否有用戶使用此角色
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? string.Empty);
            if (usersInRole.Any())
            {
                TempData["ErrorMessage"] = $"無法刪除角色 '{role.Name}'，因為有 {usersInRole.Count} 個用戶正在使用此角色";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                _logger.LogInformation($"刪除角色成功: {role.Name}");
                TempData["SuccessMessage"] = "角色刪除成功";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                TempData["ErrorMessage"] = error.Description;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Roles/ManageUsers/5
        public async Task<IActionResult> ManageUsers(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var users = _userManager.Users.ToList();
            var model = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    FullName = $"{user.FirstName ?? string.Empty} {user.LastName ?? string.Empty}",
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name ?? string.Empty)
                };
                model.Add(userRoleViewModel);
            }

            ViewBag.RoleId = id;
            ViewBag.RoleName = role.Name;

            return View(model);
        }

        // POST: /Roles/ManageUsers/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUsers(string id, List<UserRoleViewModel> model)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            int addedCount = 0;
            int removedCount = 0;

            foreach (var userRole in model)
            {
                var user = await _userManager.FindByIdAsync(userRole.UserId ?? string.Empty);
                if (user != null)
                {
                    var isInRole = await _userManager.IsInRoleAsync(user, role.Name ?? string.Empty);

                    if (userRole.IsSelected && !isInRole)
                    {
                        var result = await _userManager.AddToRoleAsync(user, role.Name ?? string.Empty);
                        if (result.Succeeded)
                        {
                            addedCount++;
                        }
                    }
                    else if (!userRole.IsSelected && isInRole)
                    {
                        var result = await _userManager.RemoveFromRoleAsync(user, role.Name ?? string.Empty);
                        if (result.Succeeded)
                        {
                            removedCount++;
                        }
                    }
                }
            }

            _logger.LogInformation($"角色 '{role.Name}' 用戶管理: 添加 {addedCount} 個用戶，移除 {removedCount} 個用戶");
            TempData["SuccessMessage"] = $"角色 '{role.Name}' 用戶管理成功: 添加 {addedCount} 個用戶，移除 {removedCount} 個用戶";
            return RedirectToAction(nameof(Index));
        }
    }
} 