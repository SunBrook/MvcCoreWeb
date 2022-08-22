using System.ComponentModel.DataAnnotations.Schema;

namespace MvcCoreWeb.Models.DbModels
{
    /// <summary>
    /// 账号实体
    /// </summary>
    [Table("Account")]
    public class Account
    {
        /// <summary>
        /// 账号Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
    }
}
