using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MvcCoreWeb.Models.DbModels
{
    /// <summary>
    /// 用户权限表
    /// </summary>
    [Table("UserPermissions")]
    public class UserPermission
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 账号Id
        /// </summary>
        public int AccountId { get; set; }
        /// <summary>
        /// 权限Id
        /// </summary>
        public int PermissionId { get; set; }
    }
}
