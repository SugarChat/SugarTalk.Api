using SugarTalk.Messages.Enums.Meeting;
using System;

namespace SugarTalk.Messages.Dto.Meetings;

public class MeetingUserSessionDto
{
    public int Id { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public Guid MeetingId { get; set; }

    public int UserId { get; set; }

    public string UserName { set; get; }
    
    /// <summary>
    /// guest custom name
    /// </summary>
    public string GuestName { get; set; }

    public bool IsMuted { get; set; }

    public bool IsSharingScreen { get; set; }
    
    public bool IsMeetingMaster { get; set; }
    
    public MeetingUserSessionOnlineType OnlineType { get; set; }

    public Guid? MeetingSubId { get; set; }
    
    public long? LastJoinTime { get; set; }
    
    public bool CoHost { get; set; }

    public DateTimeOffset? LastModifiedDateForCoHost { get; set; }

    public bool? IsMeetingCreator { get; set; }
}