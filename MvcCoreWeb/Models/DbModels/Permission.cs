using System.ComponentModel.DataAnnotations.Schema;

namespace MvcCoreWeb.Models.DbModels
{
    /// <summary>
    /// 权限表
    /// </summary>
    [Table("Permissions")]
    public class Permission
    {
        /// <summary>
        /// 权限Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 控制器
        /// </summary>
        public string Controller { get; set; }
        /// <summary>
        /// 方法
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// /控制器/方法
        /// </summary>
        public string Url { get; set; }
    }
}
