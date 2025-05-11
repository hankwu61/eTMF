using System.Collections.Generic;

namespace ETMF.Models
{
    /// <summary>
    /// 系统角色和权限定义
    /// </summary>
    public static class UserRole
    {
        // 角色定义
        public const string Admin = "Admin";
        public const string DocumentManager = "DocumentManager";
        public const string DocumentReviewer = "DocumentReviewer";
        public const string DocumentApprover = "DocumentApprover";
        public const string DocumentViewer = "DocumentViewer";

        // 权限定义
        public static class Permissions
        {
            // 文档上传权限
            public const string UploadDocuments = "UploadDocuments";
            
            // 文档审核权限
            public const string ReviewDocuments = "ReviewDocuments";
            
            // 文档批准权限
            public const string ApproveDocuments = "ApproveDocuments";
            
            // 文档退回权限
            public const string RejectDocuments = "RejectDocuments";
            
            // 文档归档权限
            public const string ArchiveDocuments = "ArchiveDocuments";
            
            // 文档删除权限
            public const string DeleteDocuments = "DeleteDocuments";
            
            // 文档查看权限
            public const string ViewDocuments = "ViewDocuments";
            
            // 用户管理权限
            public const string ManageUsers = "ManageUsers";
            
            // 角色管理权限
            public const string ManageRoles = "ManageRoles";
            
            // 系统设置权限
            public const string ManageSystem = "ManageSystem";
        }

        // 角色权限映射
        public static Dictionary<string, List<string>> RolePermissions = new Dictionary<string, List<string>>
        {
            // 管理员拥有所有权限
            {
                Admin, new List<string>
                {
                    Permissions.UploadDocuments,
                    Permissions.ReviewDocuments,
                    Permissions.ApproveDocuments,
                    Permissions.RejectDocuments,
                    Permissions.ArchiveDocuments,
                    Permissions.DeleteDocuments,
                    Permissions.ViewDocuments,
                    Permissions.ManageUsers,
                    Permissions.ManageRoles,
                    Permissions.ManageSystem
                }
            },
            
            // 文档管理员可以上传、查看和归档文档
            {
                DocumentManager, new List<string>
                {
                    Permissions.UploadDocuments,
                    Permissions.ViewDocuments,
                    Permissions.ArchiveDocuments
                }
            },
            
            // 文档审核员可以审核、退回和查看文档
            {
                DocumentReviewer, new List<string>
                {
                    Permissions.ReviewDocuments,
                    Permissions.RejectDocuments,
                    Permissions.ViewDocuments
                }
            },
            
            // 文档批准人可以批准、退回和查看文档
            {
                DocumentApprover, new List<string>
                {
                    Permissions.ApproveDocuments,
                    Permissions.RejectDocuments,
                    Permissions.ViewDocuments
                }
            },
            
            // 文档查看者只能查看文档
            {
                DocumentViewer, new List<string>
                {
                    Permissions.ViewDocuments
                }
            }
        };

        // 获取角色拥有的权限
        public static List<string> GetPermissionsForRole(string roleName)
        {
            if (RolePermissions.ContainsKey(roleName))
            {
                return RolePermissions[roleName];
            }
            
            return new List<string>();
        }
        
        // 检查角色是否拥有特定权限
        public static bool HasPermission(string roleName, string permission)
        {
            if (RolePermissions.ContainsKey(roleName))
            {
                return RolePermissions[roleName].Contains(permission);
            }
            
            return false;
        }
    }
} 