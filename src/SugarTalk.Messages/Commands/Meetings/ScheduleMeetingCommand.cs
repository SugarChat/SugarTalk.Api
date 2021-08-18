using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Messages.Commands.Meetings
{
    public class ScheduleMeetingCommand : ICommand
    {
        public Guid Id { get; set; }
        
        public MeetingType MeetingType { get; set; }
    }
}