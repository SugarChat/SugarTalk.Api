using Mediator.Net.Contracts;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Constants;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

[SugarTalkAuthorize(new string[] { }, new[] { SecurityStore.Permissions.CanGetMeetingRelatedData })]
public class GetMeetingByNumberRequest : IRequest
{
    public string MeetingNumber { get; set; }

    public bool IncludeUserSession { get; set; } = true;
}

public class GetMeetingByNumberResponse : SugarTalkResponse<MeetingDto>
{
}
