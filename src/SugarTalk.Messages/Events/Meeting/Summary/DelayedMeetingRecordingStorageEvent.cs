using System;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Events.Meeting.Summary;

public class DelayedMeetingRecordingStorageEvent : IEvent
{
    public Guid MeetingId { get; set; }

    public string EgressId { get; set; }
        
    public Guid MeetingRecordId { get; set; }
    
    public string Token { get; set; }
    
    public int ReTryLimit { get; set; } = 5;
}