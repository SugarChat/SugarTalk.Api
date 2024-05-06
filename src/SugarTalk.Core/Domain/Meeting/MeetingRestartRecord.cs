using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_restart_record")]
public class MeetingRestartRecord : IEntity
{
    public MeetingRestartRecord()
    {
        CreatedDate = DateTimeOffset.Now;
    }

    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("meeting_id")]
    public Guid MeetingId { get; set; }

    [Column("record_id")]
    public Guid RecordId { get; set; }

    [Column("url")]
    public string Url { get; set; }

    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }
}