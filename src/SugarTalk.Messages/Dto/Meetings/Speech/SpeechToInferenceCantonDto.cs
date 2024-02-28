using System.Collections.Generic;
using Newtonsoft.Json;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Extensions;

namespace SugarTalk.Messages.Dto.Meetings.Speech;

public class SpeechToInferenceCantonDto
{
    [JsonProperty("voice_id")]
    public string VoiceId { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }
   
    [JsonProperty("gender")]
    public string Gender { get; set; }
    
    [JsonProperty("response_format")]
    public string ResponseFormat { get; set; }
    
    public EchoAvatarLanguageType LanguageType { get; set; } 
    
    [JsonProperty("language")]
    public string LanguageStatus
    {
        get
        {
            return LanguageType switch
            {
                EchoAvatarLanguageType.Cantonese => EchoAvatarLanguageType.Cantonese.GetDescription(),
                EchoAvatarLanguageType.English => EchoAvatarLanguageType.English.GetDescription(),
                EchoAvatarLanguageType.Korean => EchoAvatarLanguageType.Korean.GetDescription(),
                EchoAvatarLanguageType.Spanish => EchoAvatarLanguageType.Spanish.GetDescription(),
                EchoAvatarLanguageType.Mandarin => EchoAvatarLanguageType.Mandarin.GetDescription(),
                EchoAvatarLanguageType.None => EchoAvatarLanguageType.None.GetDescription(),
            };
        }
    }
}

public class ErrorResponseDto
{
    [JsonProperty("detail")]
    public List<SpeechToInferenceCantonResponseDto> Detail { get; set; }
}

public class SpeechToInferenceCantonResponseDto 
{
    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("messages")]
    public string Messages { get; set; }
 
    [JsonProperty("result")]
    public SpeechToInferenceCantonResultDto Result { get; set; }
}

public class SpeechToInferenceCantonResultDto
{
    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("base64")]
    public string Base64 { get; set; }
}