using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_record")]
public class MeetingRecord : IEntity
{
    public MeetingRecord()
    {
        CreatedDate = DateTimeOffset.Now;
    }

    [Key]
    [Column("id", TypeName = "char(36)")]
    public Guid Id { get; set; }

    [Column("record_number"), StringLength(32)]
    public string RecordNumber { get; set; }
    
    [Column("meeting_id", TypeName = "char(36)")]
    public Guid MeetingId { get; set; }
    
    [Column("url"), StringLength(512)]
    public string Url { get; set; }
    
    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }
    
    [Column("is_deleted", TypeName = "tinyint(1)")]
    public bool IsDeleted { get; set; }
}