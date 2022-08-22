using MvcCoreWeb.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcCoreWeb.Models.DbModels
{
    /// <summary>
    /// 人员实体
    /// </summary>
    [Table("users")]
    public class User
    {
        /// <summary>
        /// 人员Id
        /// </summary>
        [Column("id")]
        public int Id { get; set; }
        /// <summary>
        /// 人员名称
        /// </summary>
        [Column("name")]
        public string Name { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [Column("sex")]
        public Sex Sex { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        [Column("isDeleted")]
        public bool IsDeleted { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("createTime")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        [Column("modifyTime")]
        public DateTime ModifyTime { get; set; }
    }
}
