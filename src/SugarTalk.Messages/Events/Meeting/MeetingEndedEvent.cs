using Mediator.Net.Contracts;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Messages.Events.Meeting;

public class MeetingEndedEvent : IEvent
{
    public EndMeetingResponseData Response { get; set; }
}