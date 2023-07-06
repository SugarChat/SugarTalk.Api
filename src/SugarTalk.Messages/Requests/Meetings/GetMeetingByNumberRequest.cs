using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingByNumberRequest : IRequest
{
    public string MeetingNumber { get; set; }

    public bool IncludeUserSession { get; set; } = true;
}

public class GetMeetingByNumberResponse : SugarTalkResponse<GetMeetingByNumberData>
{
}

public class GetMeetingByNumberData
{
    public string AppName { get; set; }
    
    public MeetingDto Meeting { get; set; }
}
