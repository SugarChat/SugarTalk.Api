using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingInvitationRecordsRequest : IRequest
{
}

public class GetMeetingInvitationRecordsResponse : SugarTalkResponse<List<GetMeetingInvitationRecordsDto>>
{
}

public class GetMeetingInvitationRecordsDto
{
    public int Id { get; set; }

    public string InvitingPeople { get; set; }
    
    public string MeetingTitle { get; set; }
}