using Serilog;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Messages.Dto.Meetings;

public class MeetingDto : MeetingBaseDto
{
    private List<MeetingUserSessionDto> _userSessions;

    //用于Ant media server
    public string AppName { get; set; }

    //用于Live Kit server
    [JsonProperty("token")]
    public string MeetingTokenFromLiveKit { get; set; }

    public List<MeetingUserSessionDto> UserSessions
    {
        get => _userSessions;
        set
        {
            if (value is { Count: > 0 })
            {
                value.ForEach(e => { e.IsMeetingMaster = e.UserId == MeetingMasterUserId; });
            }

            _userSessions = value;
        }
    }

    public int UserSessionCount => UserSessions is { Count: > 0 } ? UserSessions.Count : 0;

    public void AddUserSession(MeetingUserSessionDto userSession)
    {
        Log.Information($"User session userId:{{userId}}, meetingMasterUserId:{MeetingMasterUserId}", userSession.UserId, MeetingMasterUserId);
        
        userSession.IsMeetingMaster = userSession.UserId == MeetingMasterUserId;
        UserSessions.Add(userSession);
    }

    public void UpdateUserSession(MeetingUserSessionDto userSession)
    {
        var index = UserSessions.FindIndex(x => x.Id == userSession.Id);

        if (index > -1)
        {
            userSession.IsMeetingMaster = userSession.UserId == MeetingMasterUserId;
            UserSessions[index] = userSession;
        }
    }
}

public class MeetingBaseDto
{
    public Guid Id { get; set; }
    
    public Guid? MeetingSubId { get; set; }
    
    public int MeetingMasterUserId { get; set; }

    public Guid? MeetingRecordId { get; set; }

    public string MeetingNumber { get; set; }

    public int CreatedBy { get; set; }

    public long StartDate { get; set; }

    public long EndDate { get; set; }
    
    public long CreatorJoinTime { get; set; }
    
    public string TimeZone { get; set; }

    public string OriginAddress { get; set; }
    
    public string Title { get; set; }
    
    public string Password { get; set; }
    
    public string SecurityCode { get; set;}
    
    public MeetingRepeatType RepeatType { get; set; }
    
    public MeetingStreamMode MeetingStreamMode { get; set; }
    
    public MeetingAppointmentType AppointmentType { get; set; }
    
    public MeetingStatus Status { get; set; }

    public bool IsMuted { get; set; } = false;
    
    public bool IsRecorded { get; set; } = false;

    public List<AppointmentMeetingDetailForParticipantDto> Participants { get; set; }

    public int ParticipantsCount { get; set; }
    
    public bool? IsMetis { get; set; }
    
    public bool IsPasswordEnabled { get; set; } = false;

    public bool IsActiveEa { get; set; } = false;

    public bool IsActiveRecord { get; set; } = false;
}

public class AddOrUpdateMeetingDto
{
    public AddOrUpdateMeetingDto()
    {
        StartDate = DateTimeOffset.Now;
        EndDate = DateTimeOffset.Now.AddDays(1);
        MeetingStreamMode = MeetingStreamMode.LEGACY;
        AppointmentType = MeetingAppointmentType.Quick;
    }

    public string Title { get; set; }
    
    public string TimeZone { get; set; }
    
    public string SecurityCode { get; set;}
    
    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }
    
    public DateTimeOffset? UtilDate { get; set; }
    
    public MeetingRepeatType RepeatType { get; set; }

    public MeetingAppointmentType AppointmentType { get; set; }
    
    public MeetingStreamMode MeetingStreamMode { get; set; }

    public List<MeetingParticipantDto> Participants { get; set; }

    public bool? IsMetis { get; set; }
    
    public bool IsMuted { get; set; } = false;
    
    public bool IsRecorded { get; set; } = false;
}

public class MeetingParticipantDto
{
    public Guid ThirdPartyUserId { get; set; }

    public bool IsDesignatedHost { get; set; }
}

public class AppointmentMeetingDetailForParticipantDto
{
    public Guid ThirdPartyUserId { get; set; }
    
    public string UserName { get; set; }

    public bool IsDesignatedHost { get; set; }
}