using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class OutMeetingCommand : ICommand
{
    public int MeetingUserSessionId { get; set; }
}

public class OutMeetingResponse : SugarTalkResponse<string>
{
}