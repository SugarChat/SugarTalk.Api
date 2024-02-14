using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings.Speak;

namespace SugarTalk.Messages.Events.Meeting.Speak;

public class MeetingSpeakRecordedEvent : IEvent
{
    public MeetingSpeakDetailDto MeetingSpeakDetail { get; set; }
}