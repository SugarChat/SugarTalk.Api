using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetCurrentUserMeetingRecordRequest: IRequest
{
}

public class GetCurrentUserMeetingRecordResponse : IResponse
{
    public List<MeetingRecordDto> MeetingRecordList { get; set; }
}