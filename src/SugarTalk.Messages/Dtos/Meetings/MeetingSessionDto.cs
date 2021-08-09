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
            AllUserSessions = new List<UserSessionDto>();
            UserSessions = new ConcurrentDictionary<string, UserSessionDto>();
        }
        
        public Guid Id { get; set; }
        
        public Guid MeetingId { get; set; }
        
        public string PipelineId { get; set; }
        
        public string MeetingNumber { set; get; }
        
        public MeetingType MeetingType { get; set; }
        
        public MediaPipeline Pipeline { get; set; }
        
        public List<UserSessionDto> AllUserSessions { get; set; }

        public ConcurrentDictionary<string, UserSessionDto> UserSessions { get; set; }

        public void AddUserSession(UserSessionDto userSession)
        {
            AllUserSessions.Add(userSession);
        }

        public void UpdateUserSession(UserSessionDto userSession)
        {
            var index = AllUserSessions.FindIndex(x => x.Id == userSession.Id);

            if (index > -1)
                AllUserSessions[index] = userSession;
        }
    }
}