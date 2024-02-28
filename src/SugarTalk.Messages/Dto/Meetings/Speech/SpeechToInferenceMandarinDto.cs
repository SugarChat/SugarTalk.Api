using Newtonsoft.Json;

namespace SugarTalk.Messages.Dto.Meetings.Speech;

public class SpeechToInferenceMandarinDto
{
    [JsonProperty("voice_id")] 
    public string VoiceId { get; set; }

    [JsonProperty("username")] 
    public string UserName { get; set; }

    [JsonProperty("text")] 
    public string Text { get; set; }
}

public class SpeechToInferenceMandarinResponseDto
{
    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("messages")]
    public string Messages { get; set; }

    [JsonProperty("result")]
    public SpeechToInferenceResultDto Result { get; set; }
}

public class SpeechToInferenceResultDto
{
    [JsonProperty("url")]
    public InferredUrlObject Url { get; set; }

    public class InferredUrlObject
    {
        [JsonProperty("value")]
        public string UrlValue { get; set; }
    }
}