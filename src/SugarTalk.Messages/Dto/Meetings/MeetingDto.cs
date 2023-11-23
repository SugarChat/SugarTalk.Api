using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Messages.Dto.Meetings;

public class MeetingDto : MeetingBaseDto
{
    //用于Ant media server
    public string AppName { get; set; }

    //用于Live Kit server
    [JsonProperty("token")]
    public string MeetingTokenFormLiveKit { get; set; }
    
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

public class MeetingBaseDto
{
    public Guid Id { get; set; }
    
    public int MeetingMasterUserId { get; set; }

    public string MeetingNumber { get; set; }
    
    public string MergedStream => $"{MeetingNumber}Merged";
    
    public long StartDate { get; set; }

    public long EndDate { get; set; }
    
    public string TimeZone { get; set; }

    public string OriginAddress { get; set; }
    
    public string Title { get; set; }
    
    public MeetingPeriodType PeriodType { get; set; }
    
    public MeetingStreamMode MeetingStreamMode { get; set; }

    public bool IsMuted { get; set; } = false;
    
    public bool IsRecorded { get; set; } = false;
}

public class AddOrUpdateMeetingDto
{
    public string Title { get; set; }
    
    public string TimeZone { get; set; }
    
    public string SecurityCode { get; set;}
    
    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }
    
    public MeetingPeriodType PeriodType { get; set; }

    public MeetingStreamMode MeetingStreamMode { get; set; } = MeetingStreamMode.LEGACY;

    public bool IsMuted { get; set; } = false;
    
    public bool IsRecorded { get; set; } = false;
}
