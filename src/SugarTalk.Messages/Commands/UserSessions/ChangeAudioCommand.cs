using System;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Commands.UserSessions
{
    public class ChangeAudioCommand : ICommand
    {
        public Guid UserSessionId { get; set; }
        public bool IsMuted { get; set; }
    }
}