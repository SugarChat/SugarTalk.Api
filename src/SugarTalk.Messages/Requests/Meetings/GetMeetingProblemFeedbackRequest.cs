using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingProblemFeedbackRequest : IRequest
{
    public string KeyWord { get; set; }
    
    public int PageIndex { get; set; } = 1;

    public int PageSize { get; set; } = 15;
}

public class GetMeetingProblemFeedbackResponse : SugarTalkResponse<MeetingProblemFeedbackDto>
{
    public List<MeetingProblemFeedbackDto> Result { get; set; }
    
    public int Count { get; set; }
}