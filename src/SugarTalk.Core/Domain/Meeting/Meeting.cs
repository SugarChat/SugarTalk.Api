using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting")]
public class Meeting : IEntity
{
    public Meeting()
    {
        CreatedDate = DateTimeOffset.Now;
    }

    [Key]
    [Column("id", TypeName = "char(36)")]
    public Guid Id { get; set; }

    /// <summary>
    /// 会议主持人用户ID
    /// </summary>
    [Column("meeting_master_user_id")] 
    public int MeetingMasterUserId { get; set; }

    /// <summary>
    /// 会议创建时间
    /// </summary>
    [Column("created_date")] 
    public DateTimeOffset CreatedDate { get; set; }

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
    /// 会议号
    /// </summary>
    [Column("meeting_number"), StringLength(256)]
    public string MeetingNumber { get; set; }

    /// <summary>
    /// 源地址
    /// </summary>
    [Column("origin_address")] 
    public string OriginAddress { get; set; }

    /// <summary>
    /// 会议流模式
    /// </summary>
    [Column("meeting_stream_mode")] 
    public MeetingStreamMode MeetingStreamMode { get; set; }

    /// <summary>
    /// 会议标题
    /// </summary>
    [Column("title"), StringLength(256)] 
    public string Title { get; set; }

    /// <summary>
    /// 时区
    /// </summary>
    [Column("time_zone"), StringLength(128)]
    public string TimeZone { get; set; }

    /// <summary>
    /// 安全码
    /// </summary>

    [Column("security_code"), StringLength(128)]
    public string SecurityCode { get; set; }

    /// <summary>
    /// 会议状态
    /// </summary>
    [Column("status")]
    public MeetingStatus Status { get; set; }
    
    /// <summary>
    /// 会议预约类型
    /// </summary>
    [Column("appointment_type")]
    public MeetingAppointmentType AppointmentType { get; set; }

    /// <summary>
    /// 是否静音
    /// </summary>
    [Column("is_muted")] 
    public bool IsMuted { get; set; }

    /// <summary>
    /// 是否录制
    /// </summary>
    [Column("is_recorded")] 
    public bool IsRecorded { get; set; }
}