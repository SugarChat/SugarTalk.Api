using SugarTalk.Messages.Enums.OpenAi;

namespace SugarTalk.Messages.Dto.OpenAi;

public class OpenAiKeyDto
{
    public string ApiKey { get; set; }
    
    public string Organization { get; set; }
    
    public OpenAiProvider Provider { get; set; }
}