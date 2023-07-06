using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetSimpleMeetingRequest : IRequest
{
    public string MeetingNumber { get; set; }
}

public class GetSimpleMeetingResponse : SugarTalkResponse<GetSimpleMeetingData>
{
}

public class GetSimpleMeetingData
{
    public string AppName { get; set; }
    
    public MeetingDto Meeting { get; set; }
}
