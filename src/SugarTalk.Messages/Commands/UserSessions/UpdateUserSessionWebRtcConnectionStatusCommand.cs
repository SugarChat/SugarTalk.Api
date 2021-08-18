using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Messages.Commands.UserSessions
{
    public class UpdateUserSessionWebRtcConnectionStatusCommand : ICommand
    {
        public Guid UserSessionWebRtcConnectionId { get; set; }
        
        public UserSessionWebRtcConnectionStatus ConnectionStatus { get; set; }
    }
}