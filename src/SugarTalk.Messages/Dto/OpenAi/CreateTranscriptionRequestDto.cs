using SugarTalk.Messages.Enums.OpenAi;

namespace SugarTalk.Messages.Dto.OpenAi;

public class CreateTranscriptionRequestDto
{
    public byte[] File { get; set; }
    
    public string FileName { get; set; }
    
    public string Model { get; set; } = "whisper-1";

    public string Prompt { get; set; }
    
    public TranscriptionLanguage Language { get; set; } = TranscriptionLanguage.Chinese;
    
    public string ResponseFormat { get; set; } = "text";
}