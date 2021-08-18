using Mediator.Net.Contracts;
using SugarTalk.Messages.Dtos.Users;

namespace SugarTalk.Messages.Events.UserSessions
{
    public class UserSessionWebRtcConnectionRemovedEvent : IEvent
    {
        public UserSessionWebRtcConnectionDto RemovedConnection { get; set; }
    }
}