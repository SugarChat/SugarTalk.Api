using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_participant")]
public class MeetingParticipant : IEntity
{
    public MeetingParticipant()
    {
        CreatedDate = DateTimeOffset.Now;
    }

    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("meeting_id")]
    public Guid MeetingId { get; set; }

    [Column("is_designated_host")]
    public bool IsDesignatedHost { get; set; }
    
    [Column("staff_id")]
    public Guid StaffId { get; set; }
    
    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }
}