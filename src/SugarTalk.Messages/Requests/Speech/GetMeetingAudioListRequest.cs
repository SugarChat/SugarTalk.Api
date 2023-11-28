using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings.Speech;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Speech;

public class GetMeetingAudioListRequest : IRequest
{
    public Guid MeetingId { get; set; }

    public bool FilterHasCanceledAudio { get; set; } = true;
}

public class GetMeetingAudioListResponse : SugarTalkResponse<List<MeetingSpeechDto>>
{
}
