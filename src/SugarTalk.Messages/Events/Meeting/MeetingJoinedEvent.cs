using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Meetings.User;

namespace SugarTalk.Messages.Events.Meeting;

public class MeetingJoinedEvent : IEvent
{
    public int UserId { get; set; }

    public string UserName { get; set; }

    public bool IsEntryWaitingRoom { get; set; }

    public MeetingDto Meeting { get; set; }
    
    public MeetingUserSettingDto MeetingUserSetting { get; set; }
}