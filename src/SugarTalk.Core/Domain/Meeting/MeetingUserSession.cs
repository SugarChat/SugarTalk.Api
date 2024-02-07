using System;
using SugarTalk.Messages.Enums.Meeting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

/// <summary>
/// �û�����Ự
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
    /// ��ϯ״̬
    /// </summary>
    [Column("status")]
    public MeetingAttendeeStatus Status { get; set; }

    /// <summary>
    /// �״μ���ʱ��
    /// </summary>
    [Column("first_join_time")]
    public long? FirstJoinTime { get; set; }


    /// <summary>
    /// �ϴ��˳�ʱ��
    /// </summary>
    [Column("last_quit_time")]
    public long? LastQuitTime { get; set; }

    /// <summary>
    /// �ܼ�����
    /// </summary>
    [Column("total_join_count")]
    public int TotalJoinCount { get; set; }

    /// <summary>
    /// �ۼ�ʱ��
    /// </summary>
    [Column("cumulative_time")]
    public long? CumulativeTime { get; set; }

    /// <summary>
    /// �Ƿ���
    /// </summary>
    [Column("is_muted", TypeName = "tinyint(1)")]
    public bool IsMuted { get; set; }

    /// <summary>
    /// �Ƿ�����Ļ
    /// </summary>
    [Column("is_sharing_screen", TypeName = "tinyint(1)")]
    public bool IsSharingScreen { get; set; }

    /// <summary>
    /// �Ƿ�ɾ��
    /// </summary>
    [Column("is_deleted", TypeName = "tinyint(1)")]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// ����ʱ��
    /// </summary>
    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// �û��Ƿ�Ϊ��ǰ�����������
    /// </summary>
    [Column("is_meeting_master")]
    public bool IsMeetingMaster { get; set; } = false;

    /// <summary>
    /// �˳�����
    /// </summary>
    [Column("online_type")]
    public MeetingUserSessionOnlineType OnlineType { get; set; } = MeetingUserSessionOnlineType.Online;
}