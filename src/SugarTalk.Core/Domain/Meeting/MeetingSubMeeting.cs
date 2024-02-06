using System;
using SugarTalk.Messages.Enums.Meeting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

/// <summary>
/// �����µ��ӻ���
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
    /// ��¼�ӻ���״̬
    /// </summary>
    [Column("status")]
    public MeetingRecordSubConferenceStatus SubConferenceStatus { get; set; }
    
    /// <summary>
    /// �ӻ��鿪ʼʱ��
    /// </summary>
    [Column("start_time")]
    public long StartTime { get; set; }
    
    /// <summary>
    /// �ӻ������ʱ��
    /// </summary>
    [Column("end_time")]
    public long EndTime { get; set; }
}