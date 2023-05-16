using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Messages.Events.Meeting
{
    public class ScreenSharedEvent : IEvent
    {
        public MeetingUserSessionDto MeetingUserSession { get; set; }
    }
}