using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Messages.Events.Meeting
{
    public class ScreenSharedEvent : IEvent
    {
        public ConferenceRoomResponseBaseDto Response { get; set; }
        
        public MeetingUserSessionDto MeetingUserSession { get; set; }
    }
}