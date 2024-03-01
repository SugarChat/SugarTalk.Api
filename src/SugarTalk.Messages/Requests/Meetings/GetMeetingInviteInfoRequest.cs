using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingInviteInfoRequest : IRequest
{
    public Guid MeetingId { get; set; }
}

public class GetMeetingInviteInfoResponse : SugarTalkResponse<GetMeetingInviteInfoResponseData>
{
}

public class GetMeetingInviteInfoResponseData
{
    public string Sender { get; set; }
    
    public string Title { get; set; }
    
    public string MeetingNumber { get; set; }
    
    public string Url { get; set; }
    
    public string SecurityCode { get; set; }
}
