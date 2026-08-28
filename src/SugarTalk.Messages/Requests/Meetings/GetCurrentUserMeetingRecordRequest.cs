using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Constants;
using SugarTalk.Messages.Dto;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

[SugarTalkAuthorize(new string[] { }, new[] { SecurityStore.Permissions.CanGetMeetingRelatedData })]
public class GetCurrentUserMeetingRecordRequest : IRequest
{
    public string Keyword { get; set; }

    public string MeetingTitle { get; set; }

    public string MeetingNumber { get; set; }

    public string Creator { get; set; }

    public PageSetting PageSetting { get; set; } = new();
}

public class GetCurrentUserMeetingRecordResponse : SugarTalkResponse<GetCurrentUserMeetingRecordResponseDto>
{
}

public class GetCurrentUserMeetingRecordResponseDto
{
    public int Count { get; set; }

    public List<MeetingRecordDto> Records { get; set; }
}