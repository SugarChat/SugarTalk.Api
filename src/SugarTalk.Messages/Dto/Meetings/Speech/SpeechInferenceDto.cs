using Newtonsoft.Json;

namespace SugarTalk.Messages.Dto.Meetings.Speech;

public class SpeechInferenceDto
{
    [JsonProperty("name")] 
    public string Name { get; set; }
    
    [JsonProperty("language_id")]
    public int LanguageId { get; set; }
    
    [JsonProperty("text")]
    public string Text { get; set; }
    
    [JsonProperty("transpose")]
    public float Transpose { get; set; }
    
    [JsonProperty("speed")]
    public float Speed { get; set; }
    
    [JsonProperty("response_format")]
    public string ResponseFormat { get; set; }
}

public class SpeechInferenceResponseDto
{
    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("result")]
    public SpeechInferenceResultDto Result { get; set; }
}

public class SpeechInferenceResultDto
{
    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("base64")]
    public string Base64 { get; set; }
}