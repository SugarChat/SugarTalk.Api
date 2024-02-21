using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class MeetingInviteRequest : IRequest
{
    public string SecurityCode { get; set; }
    
    public string MeetingNumber { get; set; }
}

public class MeetingInviteResponse : SugarTalkResponse
{
    public string Token { get; set; }
    
    public bool HasMeetingPassword { get; set; }
}