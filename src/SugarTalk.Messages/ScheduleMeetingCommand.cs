
using System;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages
{
    public class ScheduleMeetingCommand: ICommand
    {
        
    }

    public class MeetingDto
    {
        public string MeetingId { get; set; }
    }
}