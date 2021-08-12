using Mediator.Net.Contracts;
using SugarTalk.Messages.Dtos.Users;

namespace SugarTalk.Messages.Events.UserSessions
{
    public class AudioChangedEvent : IEvent
    {
        public UserSessionDto UserSession { get; set; }
    }
}