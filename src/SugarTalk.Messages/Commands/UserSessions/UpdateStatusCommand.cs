using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Messages.Commands.UserSessions
{
    public class UpdateStatusCommand : ICommand
    {
        public Guid UserSessionId { get; set; }
        
        public string ConnectionId { get; set; }
        
        public UserSessionConnectionStatus ConnectionStatus { get; set; }
    }
}