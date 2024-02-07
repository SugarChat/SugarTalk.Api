using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingRecordByUserRequest: IRequest
{
}

public class GetMeetingRecordByUserResponse : IResponse
{
    public List<MeetingRecordDto> MeetingRecordList { get; set; }
}