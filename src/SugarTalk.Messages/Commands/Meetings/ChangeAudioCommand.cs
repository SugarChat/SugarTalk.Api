using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class ChangeAudioCommand : ICommand
{
    public int MeetingUserSessionId { get; set; }
 
    public string StreamId { get; set; }
    
    public bool IsMuted { get; set; }
}

public class ChangeAudioResponse : SugarTalkResponse<ChangeAudioData>
{
}

public class ChangeAudioData
{
    public ConferenceRoomResponseBaseDto Response { get; set; }
        
    public MeetingUserSessionDto MeetingUserSession { get; set; }
}
