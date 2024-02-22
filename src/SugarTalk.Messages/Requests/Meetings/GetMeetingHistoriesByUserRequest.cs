using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto;
using System.Collections.Generic;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Messages.Requests.Meetings;

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