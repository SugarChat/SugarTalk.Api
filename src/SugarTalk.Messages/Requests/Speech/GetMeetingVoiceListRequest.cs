using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Speech;

public class GetMeetingVoiceListRequest : IRequest
{
    public Guid MeetingId { get; set; }
    
    public bool FilterHasCanceledAudio { get; set; } = true;
}

public class GetMeetingVoiceListResponse : SugarTalkResponse<GetMeetingVoiceListDto>
{
}

public class GetMeetingVoiceListDto
{
    public List<SpeechDto> Speeches { get; set; }
}

public class SpeechDto
{
    public Guid Id { get; set; }
    
    public Guid MeetingId { get; set; }
    
    public int UserId { get; set; }
    
    public string UserName { get; set; }

    public string OriginalText { get; set; }

    public string TranslateText { get; set; }

    public string VoiceUrl { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public SpeechStatus Status { get; set; }

    public SpeechAudioLoadStatus LoadStatus { get; set; }
}

public class SpeechMappingDto
{
    public Guid Id { get; set; }
    
    public Guid MeetingId { get; set; }
    
    public int UserId { get; set; }

    public string OriginalText { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
    
    public SpeechStatus Status { get; set; }

    public string UserName { get; set; }
    
    public string VoiceId { get; set; }
    
    public SpeechTargetLanguageType LanguageId { get; set; }
    
    public string TranslateText { get; set; }

    public string VoiceUrl { get; set; }

    public SpeechAudioLoadStatus LoadStatus { get; set; }
}