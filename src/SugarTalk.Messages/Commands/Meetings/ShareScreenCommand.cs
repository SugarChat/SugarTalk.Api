using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Messages.Commands.Meetings;

public class ShareScreenCommand : ICommand
{
    public int MeetingUserSessionId { get; set; }
    
    public string StreamId { get; set; }
    
    public bool IsShared { get; set; }
}

public class ShareScreenResponse : SugarTalkResponse<ShareScreenResponseData>
{
}

public class ShareScreenResponseData
{
    public MeetingUserSessionDto MeetingUserSession { get; set; }
}
