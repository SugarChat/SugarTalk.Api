using System;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Messages.Dto.Meetings;

public class MeetingInvitationRecordDto
{
    public int Id { get; set; }
    
    public Guid MeetingId { get; set; }
    
    public Guid? MeetingSubId { get; set; }
    
    public int UserId { get; set; }
    
    public string UserName { get; set; }
    
    public int BeInviterUserId { get; set; }
    
    public MeetingInvitationStatus? InvitationStatus { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
}