using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using Newtonsoft.Json;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingDataRequest : IRequest
{
    public DateTimeOffset? Day { get; set; }
}

public class GetMeetingDataResponse : SugarTalkResponse<List<GetMeetingDataDto>>
{
}

public class GetMeetingDataDto
{
    public string MeetingName { get; set; }

    public Guid MeetingId { get; set; }

    public string MeetingNumber { get; set; }

    public string FoundationdId { get; set; }

    public string MeetingCreator { get; set; }

    public List<string> MeetingPartices { get; set; } = new();

    [JsonIgnore]
    public long MeetingStartTime { get; set; }
        
    public string MeetingStartTimePst => 
        TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(MeetingStartTime), 
            TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles")).ToString("HH:mm");

    public string TimeRange { get; set; }

    public int MeetingUseCount { get; set; }
    
    [JsonIgnore]
    public DateTimeOffset MeetingDate { get; set; }

    public string MeetingDatePst =>
        TimeZoneInfo.ConvertTime(MeetingDate, TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"))
            .ToString("yyyy/MM/dd");
}