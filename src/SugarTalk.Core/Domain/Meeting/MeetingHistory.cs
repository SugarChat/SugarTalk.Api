using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_history")]
public class MeetingHistory : IEntity
{
    public Guid Id { get; set; }
    
    public Guid MeetingId { get; set; }
    
    public long CreatorJoinTime { get; set; }
    
    public long CreatorEndTime { get; set; }
    
    public long Duration { get; set; }
    
    public bool IsDeleted { get; set; }
}