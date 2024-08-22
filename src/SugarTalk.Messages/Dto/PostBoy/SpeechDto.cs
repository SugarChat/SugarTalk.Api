using SugarTalk.Messages.Enums.OpenAi;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Dto.PostBoy;

public class SpeechDto
{
    public float Temperature { get; set; }
 
    public string AudioForBase64 { get; set; }
    
    public TranscriptionLanguage Language { get; set; }
}

public class SpeechResponse : SugarTalkResponse<string>
{
}