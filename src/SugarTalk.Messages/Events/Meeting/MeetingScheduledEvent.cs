using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Messages.Events.Meeting;

public class MeetingScheduledEvent : IEvent
{
    public MeetingDto Meeting { get; set; }
}