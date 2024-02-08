using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetCurrentUserMeetingRecordRequest : IRequest
{
    public PageSetting PageSetting { get; set; }
}

public class GetCurrentUserMeetingRecordResponse : SugarTalkResponse<GetCurrentUserMeetingRecordResponseDto>
{
}

public class GetCurrentUserMeetingRecordResponseDto
{
    public int Count { get; set; }

    public List<MeetingRecordDto> Records { get; set; }
}