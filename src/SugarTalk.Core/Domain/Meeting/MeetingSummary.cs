using System;
using System.ComponentModel.DataAnnotations;
using SugarTalk.Messages.Enums.Meeting.Summary;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_summary")]
public class MeetingSummary : IEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("record_id", TypeName = "char(36)")]
    public Guid RecordId { get; set; }
    
    [Column("meeting_number"), StringLength(48)]
    public string MeetingNumber { get; set; }
    
    [Column("speak_ids")]
    public string SpeakIds { get; set; }
    
    [Column("origin_text")]
    public string OriginText { get; set; }
    
    [Column("summary")]
    public string Summary { get; set; }
    
    [Column("status")]
    public SummaryStatus Status { get; set; } = SummaryStatus.Pending;
    
    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;
}