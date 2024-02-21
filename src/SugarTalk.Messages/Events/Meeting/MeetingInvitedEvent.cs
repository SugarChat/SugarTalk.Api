using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Events.Meeting;

public class MeetingInvitedEvent : IEvent
{
    public string Token { get; set; }
    
    public bool HasMeetingPassword { get; set; }
}