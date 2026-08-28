using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto;
using System.Collections.Generic;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Constants;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Messages.Requests.Meetings;

[SugarTalkAuthorize(new string[] { }, new[] { SecurityStore.Permissions.CanGetMeetingRelatedData })]
public class GetMeetingHistoriesByUserRequest : IRequest
{
    public string Keyword { get; set; }

    public PageSetting PageSetting { get; set; }
}

public class GetMeetingHistoriesByUserResponse : SugarTalkResponse
{
    public List<MeetingHistoryDto> MeetingHistoryList { get; set; }
    
    public int TotalCount { get; set; }
}