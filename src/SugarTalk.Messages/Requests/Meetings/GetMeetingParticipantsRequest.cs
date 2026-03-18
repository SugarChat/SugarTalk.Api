using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using Newtonsoft.Json;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingParticipantsRequest : IRequest
{
}

public class GetMeetingParticipantsResponse : SugarTalkResponse<List<GetMeetingParticipantsItemDto>>
{
}

public class GetMeetingParticipantsItemDto
{
    public Guid MeetingId { get; set; }

    [JsonIgnore]
    public long StartDateUnix { get; set; }

    [JsonIgnore]
    public long EndDateUnix { get; set; }

    [JsonIgnore]
    public DateTimeOffset? RepeatUntilDate { get; set; }

    public DateTimeOffset ActMeetingStartTimePst { get; set; }

    public DateTimeOffset ActMeetingEndTimePst { get; set; }

    public DateTimeOffset? MeetingEndTimePst { get; set; }
    
    public int MeetingDuration { get; set;  }

    public List<GetMeetingParticipantDto> MeetingParticipants { get; set; } = new();
}

public class GetMeetingParticipantDto
{
    public Guid StaffId { get; set; }

    public string StaffName { get; set; }
}
