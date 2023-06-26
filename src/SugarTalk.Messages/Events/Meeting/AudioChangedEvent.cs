using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Messages.Events.Meeting
{
    public class AudioChangedEvent : IEvent
    {
        public MeetingUserSessionDto MeetingUserSession { get; set; }
    }
}