using System;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Commands.UserSessions
{
    public class ShareScreenCommand : ICommand
    {
        public Guid UserSessionId { get; set; }
        
        public bool IsShared { get; set; }
    }
}