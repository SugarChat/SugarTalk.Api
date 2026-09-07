using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using Newtonsoft.Json;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Constants;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

[SugarTalkAuthorize(new string[] { }, new[] { SecurityStore.Permissions.CanGetMeetingRelatedData })]
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

    public string FundationId { get; set; }

    public string MeetingCreator { get; set; }

    public List<string> MeetingPartices { get; set; } = new();

    public List<string> ActMeetingPartices { get; set; } = new();

    [JsonIgnore]
    public long MeetingStartTime { get; set; }

    [JsonIgnore]
    public long MeetingEndTime { get; set; }

    [JsonIgnore]
    public DateTimeOffset? RepeatUntilDate { get; set; }

    public string MeetingStartTimePst =>
        TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(MeetingStartTime),
            TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles")).ToString("yyyy-MM-dd HH:mm:ss");

    public DateTimeOffset ActMeetingStartTimePst { get; set; }

    public DateTimeOffset ActMeetingEndTimePst { get; set; }

    public DateTimeOffset? MeetingEndTimePst { get; set; }

    public int MeetingDuration { get; set; }

    public string TimeRange { get; set; }

    public MeetingAppointmentType AppointmentType { get; set; }

    public int MeetingUseCount { get; set; }
    
    [JsonIgnore]
    public DateTimeOffset MeetingDate { get; set; }

    [JsonIgnore]
    public string UserId { get; set; }

    public string MeetingDatePst =>
        TimeZoneInfo.ConvertTime(MeetingDate, TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"))
            .ToString("yyyy/MM/dd");
}
