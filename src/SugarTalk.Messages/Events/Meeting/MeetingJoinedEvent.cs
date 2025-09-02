using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Meetings.User;
using SugarTalk.Messages.Enums.Tencent;

namespace SugarTalk.Messages.Events.Meeting;

public class MeetingJoinedEvent : IEvent
{
    public int UserId { get; set; }
    
    public string TaskId { get; set; }
    
    public MeetingDto Meeting { get; set; }
    
    public ScreenRecordingResolution RecordingResolution { get; set; }
    
    public MeetingUserSettingDto MeetingUserSetting { get; set; }
}