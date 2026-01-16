using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.MeetingMonitor;

[Table("meeting_monitor_knowledge")]
public class MeetingMonitorKnowledge : IEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("prompt")]
    public string Prompt { get; set; }

    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;
}