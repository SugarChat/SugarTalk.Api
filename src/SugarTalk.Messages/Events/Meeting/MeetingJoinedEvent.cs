using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Meetings.User;

namespace SugarTalk.Messages.Events.Meeting;

public class MeetingJoinedEvent : IEvent
{
    public MeetingDto Meeting { get; set; }
    
    public MeetingUserSettingDto MeetingUserSetting { get; set; }
    
    public ConferenceRoomResponseBaseDto Response { get; set; }
}