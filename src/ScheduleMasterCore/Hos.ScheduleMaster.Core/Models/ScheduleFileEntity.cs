using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hos.ScheduleMaster.Core.Models
{
    /// <summary>
    /// 上传文件
    /// </summary>
    [Table("schedulefiles")]
    public class ScheduleFileEntity: IEntity
    {
        public ScheduleFileEntity()
        {
            this.Id = Guid.NewGuid();
        }
        
        /// <summary>
        /// 文件id
        /// </summary>
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("createtime")]
        public DateTime CreateTime { get; set; }
        
        /// <summary>
        /// 创建人id
        /// </summary>
        [Column("createuserid")]
        public int CreateUserId { get; set; }
        
        /// <summary>
        /// 创建人账号
        /// </summary>
        [Column("createusername")]
        [MaxLength(50)]
        public string CreateUserName { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("content")]
        public byte[] Content { get; set; }
    }
}