using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Kurento.NET;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Enums;

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
        
        public MediaPipeline Pipeline { get; set; }
        
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