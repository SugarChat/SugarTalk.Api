using System;
using SugarTalk.Messages.Enums.Meeting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

/// <summary>
/// 会议下的子会议
/// </summary>
[Table("meeting_sub_meeting")]
public class MeetingSubMeeting : IEntity
{
    [Key]
    [Column("id", TypeName = "char(36)")]
    public Guid Id { get; set; }
    
    [Column("meeting_id", TypeName = "char(36)")]
    public Guid MeetingId { get; set; }
    
    /// <summary>
    /// 记录子会议状态
    /// </summary>
    [Column("status")]
    public MeetingRecordSubConferenceStatus SubConferenceStatus { get; set; }
    
    /// <summary>
    /// 子会议开始时间
    /// </summary>
    [Column("start_time")]
    public long StartTime { get; set; }
    
    /// <summary>
    /// 子会议结束时间
    /// </summary>
    [Column("end_time")]
    public long EndTime { get; set; }
}