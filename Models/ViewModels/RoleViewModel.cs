using System.ComponentModel.DataAnnotations;

namespace ETMF.Models.ViewModels
{
    public class RoleViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "角色名稱不能為空")]
        [Display(Name = "角色名稱")]
        public string Name { get; set; } = string.Empty;
    }
    
    public class UserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
} 