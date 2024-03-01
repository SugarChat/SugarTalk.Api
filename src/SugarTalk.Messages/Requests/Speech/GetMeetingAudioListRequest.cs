using System;
using Mediator.Net.Contracts;
using System.Collections.Generic;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Dto.Meetings.Speech;

namespace SugarTalk.Messages.Requests.Speech;

[AllowGuestAccess]
public class GetMeetingAudioListRequest : IRequest
{
    public Guid MeetingId { get; set; }
    
    public Guid? MeetingSubId { get; set; }
    
    //我听到的语种设置
    public SpeechTargetLanguageType LanguageType { get; set; } = SpeechTargetLanguageType.Cantonese;

    public bool FilterHasCanceledAudio { get; set; } = true;
}

public class GetMeetingAudioListResponse : SugarTalkResponse<List<MeetingSpeechDto>>
{
}
