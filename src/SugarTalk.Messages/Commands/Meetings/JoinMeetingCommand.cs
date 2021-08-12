using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Commands.Meetings
{
    public class JoinMeetingCommand : ICommand
    {
        public string MeetingNumber { get; set; }
        
        public bool IsMuted { get; set; }
    }
}