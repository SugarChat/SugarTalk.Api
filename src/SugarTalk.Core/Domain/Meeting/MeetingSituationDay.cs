using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_situation_day")]
public class MeetingSituationDay : IEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("meeting_id")]
    public Guid MeetingId { get; set; }

    [Column("time_period")]
    public string TimePeriod { get; set; }

    [Column("use_count")]
    public int UseCount { get; set; }

    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;
}