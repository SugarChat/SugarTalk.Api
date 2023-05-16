using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Events.Meeting;

public class MeetingOutedEvent : IEvent
{
    public bool IsOuted { get; set; }
}