using System;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Dto.Meetings;

public class MeetingRecordingStartedEvent : IEvent
{
    public Guid MeetingRecordId { get; set; }
    
    public string EgressId { get; set; }
}