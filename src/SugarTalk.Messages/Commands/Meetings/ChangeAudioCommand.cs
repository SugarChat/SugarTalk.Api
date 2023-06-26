using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Messages.Commands.Meetings;

public class ChangeAudioCommand : ICommand
{
    public int MeetingUserSessionId { get; set; }
 
    public string StreamId { get; set; }
    
    public bool IsMuted { get; set; }
}

public class ChangeAudioResponse : SugarTalkResponse<ChangeAudioResponseData>
{
}

public class ChangeAudioResponseData
{
    public MeetingUserSessionDto MeetingUserSession { get; set; }
}
