using System;
using Newtonsoft.Json;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Messages.Dto.Meetings.Speech;

public class MeetingSpeechDto
{
    public Guid Id { get; set; }

    public Guid MeetingId { get; set; }
    
    public int UserId { get; set; }
    
    public string UserName { get; set; }
    
    public string VoiceUrl { get; set; }
    
    public string OriginalText { get; set; }

    public string TranslatedText { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
    
    public SpeechStatus Status { get; set; }

    public MeetingChatVoiceRecordDto VoiceRecord { get; set; }
}

public class SpeechToTextDto
{
    [JsonProperty("source")]
    public Source Source { get; set; }

    [JsonProperty("response_format")] 
    public string ResponseFormat { get; set; }

    [JsonProperty("language_id")] 
    public int LanguageId { get; set; }
}

public class TextToSpeechDto
{
    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("voice_id")]
    public int VoiceId {
        get
        {
            if (MandarinToneType != null)
                return (int)MandarinToneType;
            if (CantoneseToneType != null)
                return (int)CantoneseToneType;
            if (EnglishToneType != null)
                return (int)EnglishToneType;
            if (SpanishToneType != null)
                return (int)SpanishToneType;
            return 110;
        }
    }

    [JsonProperty("file_format")] 
    public string FileFormat { get; set; } = "wav";

    [JsonProperty("response_format")] 
    public string ResponseFormat { get; set; } = "url";
    
    [JsonIgnore]
    public MandarinToneType? MandarinToneType { get; set; }
    
    [JsonIgnore]
    public EnglishToneType? EnglishToneType { get; set; }
    
    [JsonIgnore]
    public CantoneseToneType? CantoneseToneType { get; set; }
    
    [JsonIgnore]
    public SpanishToneType? SpanishToneType { get; set; }
}

public class TextTranslationDto
{
    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("target_language_id")]
    public SpeechTargetLanguageType TargetLanguageType { get; set; }
}

public class Source
{
    [JsonProperty("base64", NullValueHandling = NullValueHandling.Ignore)]
    public Base64EncodedAudio Base64 { get; set; }

    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
    public string Url { get; set; }
}

public class Base64EncodedAudio
{
    [JsonProperty("encoded")]
    public string Encoded { get; set; }

    [JsonProperty("file_format")]
    public string FileFormat { get; set; }
}

public class SpeechResponseDto
{
    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }
    
    [JsonProperty("result")]
    public string Result { get; set; }
}
