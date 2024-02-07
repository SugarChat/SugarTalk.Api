using System;
using SugarTalk.Messages.Enums.Meeting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

/// <summary>
/// 用户会议会话
/// </summary>
[Table("meeting_user_session")]
public class MeetingUserSession : IEntity
{
    public MeetingUserSession()
    {
        CreatedDate = DateTimeOffset.Now;
    }

    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("meeting_id", TypeName = "char(36)")]
    public Guid MeetingId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// 出席状态
    /// </summary>
    [Column("status")]
    public MeetingAttendeeStatus Status { get; set; }

    /// <summary>
    /// 首次加入时间
    /// </summary>
    [Column("first_join_time")]
    public long? FirstJoinTime { get; set; }


    /// <summary>
    /// 上次退出时间
    /// </summary>
    [Column("last_quit_time")]
    public long? LastQuitTime { get; set; }

    /// <summary>
    /// 总加入数
    /// </summary>
    [Column("total_join_count")]
    public int TotalJoinCount { get; set; }

    /// <summary>
    /// 累计时间
    /// </summary>
    [Column("cumulative_time")]
    public long? CumulativeTime { get; set; }

    /// <summary>
    /// 是否静音
    /// </summary>
    [Column("is_muted", TypeName = "tinyint(1)")]
    public bool IsMuted { get; set; }

    /// <summary>
    /// 是否共享屏幕
    /// </summary>
    [Column("is_sharing_screen", TypeName = "tinyint(1)")]
    public bool IsSharingScreen { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    [Column("is_deleted", TypeName = "tinyint(1)")]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// 用户是否为当前会议的主持人
    /// </summary>
    [Column("is_meeting_master")]
    public bool IsMeetingMaster { get; set; } = false;

    /// <summary>
    /// 退出类型
    /// </summary>
    [Column("online_type")]
    public MeetingUserSessionOnlineType OnlineType { get; set; } = MeetingUserSessionOnlineType.Online;
}