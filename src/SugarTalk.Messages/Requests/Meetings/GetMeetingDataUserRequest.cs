using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using Newtonsoft.Json;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingDataUserRequest : IRequest
{
    public DateTimeOffset? Day { get; set; }
}

public class GetMeetingDataUserResponse : SugarTalkResponse<List<GetMeetingDataUserDto>>
{
}

public class GetMeetingDataUserDto
{
    public Guid MeetingId { get; set; }
    
    public string MeetingNumber { get; set; }

    public string FundationId { get; set; }

    public string UserName { get; set; }

    [JsonIgnore]
    public long MeetingStartTime { get; set; }
    
    public string MeetingStartTimePst => 
        TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(MeetingStartTime), 
            TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles")).ToString("HH:mm");

    [JsonIgnore]
    public string UserId { get; set; }
    
    [JsonIgnore]
    public DateTimeOffset Date { get; set; }

    [JsonIgnore]
    public MeetingAppointmentType AppointmentType { get; set; }
    
    public string MeetingDatePst =>
        TimeZoneInfo.ConvertTime(Date, TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"))
            .ToString("yyyy/MM/dd");

    [JsonIgnore]
    public DateTimeOffset ActStartTime { get; set; }

    public string ActStartTimePst =>
        TimeZoneInfo.ConvertTime(ActStartTime, TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"))
            .ToString("yyyy-MM-dd HH:mm:ss");

    [JsonIgnore]
    public DateTimeOffset ActEndTime { get; set; }

    public string ActEndTimePst =>
        TimeZoneInfo.ConvertTime(ActEndTime, TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"))
            .ToString("yyyy-MM-dd HH:mm:ss");
}
