using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugarTalk.Core.Domain.Meeting
{
    public class MeetingStorageVideo : IEntity
    {

        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("meeting_id", TypeName = "char(36)")]
        public Guid MeetingId { get; set; }

        /// <summary>
        /// 会议号
        /// </summary>
        [Column("meeting_number"), StringLength(256)]
        public string MeetingNumber { get; set; }

        /// <summary>
        /// 会议标题
        /// </summary>
        [Column("meeting_title")]
        public string MeetingTitle { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        [Column("start_date")]
        public long StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        [Column("end_date")]
        public long EndDate { get; set; }

        /// <summary>
        /// 视频Url
        /// </summary>
        [Column("video_url")]
        public string VideoUrl { get; set; }
    }
}
