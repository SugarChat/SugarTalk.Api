using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Commands.UserSessions
{
    public class RemoveUserSessionWebRtcConnectionCommand : ICommand
    {
        public string WebRtcPeerConnectionId { get; set; }
    }
}