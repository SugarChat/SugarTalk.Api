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
    /// �����������û�ID
    /// </summary>
    [Column("meeting_master_user_id")] 
    public int MeetingMasterUserId { get; set; }

    /// <summary>
    /// ���鴴��ʱ��
    /// </summary>
    [Column("created_date")] 
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// ��ʼ����
    /// </summary>
    [Column("start_date")] 
    public long StartDate { get; set; }

    /// <summary>
    /// ��������
    /// </summary>
    [Column("end_date")] 
    public long EndDate { get; set; }

    /// <summary>
    /// �����
    /// </summary>
    [Column("meeting_number"), StringLength(256)]
    public string MeetingNumber { get; set; }

    /// <summary>
    /// Դ��ַ
    /// </summary>
    [Column("origin_address")] 
    public string OriginAddress { get; set; }

    /// <summary>
    /// ������ģʽ
    /// </summary>
    [Column("meeting_stream_mode")] 
    public MeetingStreamMode MeetingStreamMode { get; set; }

    /// <summary>
    /// �������
    /// </summary>
    [Column("title"), StringLength(256)] 
    public string Title { get; set; }

    /// <summary>
    /// ʱ��
    /// </summary>
    [Column("time_zone"), StringLength(128)]
    public string TimeZone { get; set; }

    /// <summary>
    /// ��ȫ��
    /// </summary>

    [Column("security_code"), StringLength(128)]
    public string SecurityCode { get; set; }

    /// <summary>
    /// ����״̬
    /// </summary>
    [Column("status")]
    public MeetingStatus Status { get; set; }
    
    /// <summary>
    /// ����ԤԼ����
    /// </summary>
    [Column("appointment_type")]
    public MeetingAppointmentType AppointmentType { get; set; }

    /// <summary>
    /// �Ƿ���
    /// </summary>
    [Column("is_muted")] 
    public bool IsMuted { get; set; }

    /// <summary>
    /// �Ƿ�¼��
    /// </summary>
    [Column("is_recorded")] 
    public bool IsRecorded { get; set; }
}