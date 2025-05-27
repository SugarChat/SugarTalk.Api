using System;
using System.Collections.Generic;
using SugarTalk.Messages.Enums.Meeting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_repeat_rule")]
public class MeetingRepeatRule : IEntity
{
    [Key]
    [Column("id", TypeName = "char(36)")]
    public Guid Id { get; set; }
    
    [Column("meeting_id", TypeName = "char(36)")]
    public Guid MeetingId { get; set; }
    
    [Column("repeat_type")] 
    public MeetingRepeatType RepeatType { get; set; }
    
    [Column("customize_repeat_type")]
    public MeetingCustomizeRepeatType? CustomizeRepeatType { get; set; }
    
    [Column("repeat_interval")]
    public int? RepeatInterval { get; set; }
    
    [Column("repeat_weekdays")]
    public string RepeatWeekdays { get; set; }
    
    [Column("repeat_month_days")]
    public string RepeatMonthDays { get; set; }
    
    [Column("repeat_until_date")]
    public DateTimeOffset? RepeatUntilDate { get; set; }
}