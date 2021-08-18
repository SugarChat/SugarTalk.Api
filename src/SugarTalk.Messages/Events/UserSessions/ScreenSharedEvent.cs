using Mediator.Net.Contracts;
using SugarTalk.Messages.Dtos.Users;

namespace SugarTalk.Messages.Events.UserSessions
{
    public class ScreenSharedEvent : IEvent
    {
        public UserSessionDto UserSession { get; set; }
    }
}