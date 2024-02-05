using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Meetings.User;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class JoinMeetingCommand : ICommand
{
    public string MeetingNumber { get; set; }
    
    public string SecurityCode { get; set; }
    
    public bool IsMuted { get; set; }
}

public class JoinMeetingResponse : SugarTalkResponse<JoinMeetingResponseData>
{
}

public class JoinMeetingResponseData
{
    public MeetingDto Meeting { get; set; }
    
    public MeetingUserSettingDto MeetingUserSetting { get; set; }
}
