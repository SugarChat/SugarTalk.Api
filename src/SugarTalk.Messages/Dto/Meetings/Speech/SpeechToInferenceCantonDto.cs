using Newtonsoft.Json;

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