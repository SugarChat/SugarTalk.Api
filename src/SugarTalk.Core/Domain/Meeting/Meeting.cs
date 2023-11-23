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

    [Column("meeting_master_user_id")] 
    public int MeetingMasterUserId { get; set; }

    [Column("created_date")] 
    public DateTimeOffset CreatedDate { get; set; }

    [Column("start_date")] 
    public long StartDate { get; set; }

    [Column("end_date")] 
    public long EndDate { get; set; }

    [Column("meeting_number"), StringLength(256)]
    public string MeetingNumber { get; set; }

    [Column("origin_address")] 
    public string OriginAddress { get; set; }

    [Column("meeting_stream_mode")] 
    public MeetingStreamMode MeetingStreamMode { get; set; }

    [Column("title"), StringLength(256)] 
    public string Title { get; set; }

    [Column("time_zone"), StringLength(128)]
    public string TimeZone { get; set; }

    [Column("security_code"), StringLength(128)]
    public string SecurityCode { get; set; }

    [Column("period_type")] 
    public MeetingPeriodType PeriodType { get; set; }

    [Column("is_muted")] 
    public bool IsMuted { get; set; }

    [Column("is_recorded")] 
    public bool IsRecorded { get; set; }

    [Column("is_active")] 
    public bool IsActive { get; set; }
}