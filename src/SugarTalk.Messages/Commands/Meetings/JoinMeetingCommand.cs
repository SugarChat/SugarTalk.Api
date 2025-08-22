using Mediator.Net.Contracts;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Meetings.User;
using SugarTalk.Messages.Enums.Tencent;
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
    
    public MeetingDto Meeting { get; set; }
    
    public string TaskId { get; set; }

    public ScreenRecordingResolution RecordingResolution { get; set; }
    
    public MeetingUserSettingDto MeetingUserSetting { get; set; }
}
