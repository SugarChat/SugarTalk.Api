using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Messages.Events.Meeting;

public class MeetingOutedEvent : IEvent
{
    public ConferenceRoomBaseDto Data { get; set; }
}