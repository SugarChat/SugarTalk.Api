using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class JoinMeetingCommand : ICommand
{
    public string MeetingNumber { get; set; }

    public bool IsMuted { get; set; }
}

public class JoinMeetingResponse : SugarTalkResponse<MeetingDto>
{
}
