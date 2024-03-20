using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings.Speech;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Speech;

public class GetMeetingChatVoiceRecordRequest : IRequest
{
    public Guid MeetingId { get; set; }
    
    public SpeechTargetLanguageType ListenLanguage { get; set; } = SpeechTargetLanguageType.Cantonese;
    
    public bool FilterHasCanceledAudio { get; set; } = true;
}

public class GetMeetingChatVoiceRecordResponse : SugarTalkResponse<List<MeetingSpeechDto>>
{
}