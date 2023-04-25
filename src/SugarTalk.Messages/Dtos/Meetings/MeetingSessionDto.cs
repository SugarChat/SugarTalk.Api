using System;
using System.Collections.Generic;
using SugarTalk.Messages.Dtos.AntMedia;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Messages.Dtos.Meetings
{
    public class MeetingSessionDto
    {
        public MeetingSessionDto()
        {
            UserSessions = new List<UserSessionDto>();
        }
        
        public Guid Id { get; set; }
        
        public Guid MeetingId { get; set; }
        
        public string PipelineId { get; set; }
        
        public string MeetingNumber { set; get; }
        
        public MeetingType MeetingType { get; set; }
        
        public ConferenceRoomDto room { get; set; }
        
        public List<UserSessionDto> UserSessions { get; set; }

        public void AddUserSession(UserSessionDto userSession)
        {
            UserSessions.Add(userSession);
        }

        public void UpdateUserSession(UserSessionDto userSession)
        {
            var index = UserSessions.FindIndex(x => x.Id == userSession.Id);

            if (index > -1)
                UserSessions[index] = userSession;
        }
    }
}