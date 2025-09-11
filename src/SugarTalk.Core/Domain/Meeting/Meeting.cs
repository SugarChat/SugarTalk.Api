using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting")]
public class Meeting : IEntity, IHasCreatedFields
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

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("created_date")] 
    public DateTimeOffset CreatedDate { get; set; }

    [Column("start_date")] 
    public long StartDate { get; set; }

    [Column("end_date")] 
    public long EndDate { get; set; }
    
    [Column("creator_join_time")]
    public long? CreatorJoinTime { get; set; }

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
    
    [Column("password"), StringLength(128)]
    public string Password { get; set; }

    [Column("status")]
    public MeetingStatus Status { get; set; }
    
    [Column("appointment_type")]
    public MeetingAppointmentType AppointmentType { get; set; }

    [Column("is_metis")]
    public bool? IsMetis { get; set; }
    
    [Column("is_muted")] 
    public bool IsMuted { get; set; }

    [Column("is_recorded")] 
    public bool IsRecorded { get; set; }

    [Column("is_active_ea")]
    public bool IsActiveEa { get; set; }

    [Column("is_active_record")]
    public bool IsActiveRecord { get; set; }
    
    [Column("is_locked")]
    public bool IsLocked { get; set; }

    [Column("is_waiting_room_enabled")]
    public bool IsWaitingRoomEnabled { get; set; }
}