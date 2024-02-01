using System;
using SugarTalk.Messages.Enums.Meeting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_period_rule")]
public class MeetingPeriodRule : IEntity
{
    [Key]
    [Column("id", TypeName = "char(36)")]
    public Guid Id { get; set; }
    
    [Column("meeting_id", TypeName = "char(36)")]
    public Guid MeetingId { get; set; }
    
    [Column("period_type")] 
    public MeetingPeriodType PeriodType { get; set; }
    
    [Column("until_date")]
    public DateTimeOffset? UntilDate { get; set; }
}