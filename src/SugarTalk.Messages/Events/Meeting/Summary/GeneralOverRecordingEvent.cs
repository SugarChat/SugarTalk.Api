using System;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Events.Meeting;

public class GeneralOverRecordingEvent : IEvent
{
    public Guid MeetingId { get; set; }

    public Guid MeetingRecordId { get; set; }
}