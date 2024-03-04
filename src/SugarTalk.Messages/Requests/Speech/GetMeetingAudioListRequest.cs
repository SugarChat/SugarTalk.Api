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
    
    //我听到的语种设置
    public SpeechTargetLanguageType LanguageType { get; set; } = SpeechTargetLanguageType.Cantonese;

    public bool FilterHasCanceledAudio { get; set; } = true;
}

public class GetMeetingAudioListResponse : SugarTalkResponse<List<MeetingSpeechDto>>
{
}

public class SpeechMappingDto
{
    public Guid Id { get; set; }

    public Guid MeetingId { get; set; }

    public string UserName { get; set; }

    public string VoiceId { get; set; }
    
    public int UserId { get; set; }

    public string OriginalText { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public SpeechStatus Status { get; set; }
    
    public SpeechTargetLanguageType LanguageId { get; set; }

    public string TranslateText { get; set; }

    public string VoiceUrl { get; set; }

    public SpeechAudioLoadStatus LoadStatus { get; set; }
}