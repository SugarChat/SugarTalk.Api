using System;
using System.Collections.Generic;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Messages.Dto.Meetings;

public class MeetingDto
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// 会议主持人Id
    /// </summary>
    public int MeetingMasterId { get; set; }

    public MeetingStreamMode MeetingStreamMode { get; set; }

    public string MeetingNumber { get; set; }
    
    public string MergedStream { get; set; }

    public long StartDate { get; set; }

    public long EndDate { get; set; }

    public string Mode { get; set; }

    public string OriginAddress { get; set; }
    
    public List<MeetingUserSessionDto> UserSessions { get; set; }

    public void AddUserSession(MeetingUserSessionDto userSession)
    {
        UserSessions.Add(userSession);
    }

    public void UpdateUserSession(MeetingUserSessionDto userSession)
    {
        var index = UserSessions.FindIndex(x => x.Id == userSession.Id);

        if (index > -1)
            UserSessions[index] = userSession;
    }
}
