using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetAllMeetingUserSessionsForMeetingIdRequest : IRequest
{
    public Guid MeetingId { get; set; }

    public Guid? MeetingSubId { get; set; }
}

public class GetAllMeetingUserSessionsForMeetingIdResponse : SugarTalkResponse<GetAllMeetingUserSessionsForMeetingIdDto>
{
}

public class GetAllMeetingUserSessionsForMeetingIdDto
{
    public List<MeetingUserSessionDto> MeetingUserSessions { get; set; }

    public int InMeetingCount { get; set; }

    public List<MeetingUserSessionDto> WaitingRoomUserSessions { get; set; }

    public int WaitingRoomCount { get; set; }

    public List<NoJoinMeetingUserSessionsDto> NoJoinMeetingUsers { get; set; }

    public int NoEntryMeetingCount { get; set;}
    
    public int Count { get; set; }
}

public class NoJoinMeetingUserSessionsDto
{
    public int Id { get; set; }
    
    public string UserName { get; set; }
    
    public MeetingInvitationStatus? InvitationStatus { get; set; }
}