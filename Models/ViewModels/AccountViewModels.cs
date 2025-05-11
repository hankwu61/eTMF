using System.ComponentModel.DataAnnotations;

namespace ETMF.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "請輸入電子郵件")]
        [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
        [Display(Name = "電子郵件")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "記住我")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "請輸入電子郵件")]
        [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
        [Display(Name = "電子郵件")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入名字")]
        [Display(Name = "名字")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入姓氏")]
        [Display(Name = "姓氏")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入密碼")]
        [StringLength(100, ErrorMessage = "{0}必須至少包含{2}個字符，且不超過{1}個字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "確認密碼")]
        [Compare("Password", ErrorMessage = "密碼和確認密碼不匹配。")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ProfileViewModel
    {
        [Required(ErrorMessage = "請輸入電子郵件")]
        [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
        [Display(Name = "電子郵件")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入名字")]
        [Display(Name = "名字")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入姓氏")]
        [Display(Name = "姓氏")]
        public string LastName { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "請輸入當前密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "當前密碼")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入新密碼")]
        [StringLength(100, ErrorMessage = "{0}必須至少包含{2}個字符，且不超過{1}個字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "新密碼")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "確認新密碼")]
        [Compare("NewPassword", ErrorMessage = "新密碼和確認密碼不匹配。")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
} 