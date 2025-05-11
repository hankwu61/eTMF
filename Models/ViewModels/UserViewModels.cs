using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ETMF.Models.ViewModels
{
    /// <summary>
    /// 用户列表视图模型
    /// </summary>
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Display(Name = "用户名")]
        public string UserName { get; set; } = string.Empty;
        
        [Display(Name = "电子邮件")]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "姓名")]
        public string FullName { get; set; } = string.Empty;
        
        [Display(Name = "是否激活")]
        public bool IsActive { get; set; }
        
        [Display(Name = "角色")]
        public List<string> Roles { get; set; } = new List<string>();
        
        [Display(Name = "创建时间")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "最后登录")]
        public DateTime? LastLoginAt { get; set; }
    }
    
    /// <summary>
    /// 创建用户视图模型
    /// </summary>
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "请输入电子邮件")]
        [EmailAddress(ErrorMessage = "请输入有效的电子邮件地址")]
        [Display(Name = "电子邮件")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "请输入名字")]
        [Display(Name = "名字")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "请输入姓氏")]
        [Display(Name = "姓氏")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "请输入密码")]
        [StringLength(100, ErrorMessage = "{0}必须至少包含{2}个字符", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; } = string.Empty;
        
        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [Display(Name = "是否激活")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "角色")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// 编辑用户视图模型
    /// </summary>
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "请输入电子邮件")]
        [EmailAddress(ErrorMessage = "请输入有效的电子邮件地址")]
        [Display(Name = "电子邮件")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "请输入名字")]
        [Display(Name = "名字")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "请输入姓氏")]
        [Display(Name = "姓氏")]
        public string LastName { get; set; } = string.Empty;
        
        [Display(Name = "是否激活")]
        public bool IsActive { get; set; }
        
        [Display(Name = "角色")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// 重置密码视图模型
    /// </summary>
    public class ResetPasswordViewModel
    {
        public string? Id { get; set; }
        
        [Display(Name = "电子邮件")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "请输入新密码")]
        [StringLength(100, ErrorMessage = "{0}必须至少包含{2}个字符", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; } = string.Empty;
        
        [DataType(DataType.Password)]
        [Display(Name = "确认新密码")]
        [Compare("NewPassword", ErrorMessage = "新密码和确认密码不匹配")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
} 