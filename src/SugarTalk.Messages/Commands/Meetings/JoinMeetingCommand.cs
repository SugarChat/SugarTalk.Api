using Mediator.Net.Contracts;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Meetings.User;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

[AllowGuestAccess]
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
    public int UserId { get; set; }

    public string UserName { get; set; }
    
    public bool IsEntryWaitingRoom { get; set; }
    
    public MeetingDto Meeting { get; set; }
    
    public string TaskId { get; set; }
    
    public MeetingUserSettingDto MeetingUserSetting { get; set; }
}
