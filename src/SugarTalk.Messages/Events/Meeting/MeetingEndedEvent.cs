using System.Collections.Generic;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Events.Meeting;

public class MeetingEndedEvent : IEvent
{
    public string MeetingNumber { get; set; }
    
    public List<int> MeetingUserSessionIds { get; set; }
}