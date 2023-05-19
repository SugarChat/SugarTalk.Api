using Mediator.Net.Contracts;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Messages.Events.Meeting;

public class MeetingOutedEvent : IEvent
{
    public OutMeetingData Data { get; set; }
}