using SugarTalk.Messages.Enums.Smarties;

namespace SugarTalk.Messages.Dto.Smarties;

public class CreateSpeechMaticsJobCommandDto
{
    public string Url { get; set; }
    
    public byte[] RecordContent { get; set; }

    public string RecordName { get; set; }

    public string Language { get; set; }

    public TranscriptionJobSystem SourceSystem { get; set; }
    
    public string Key { get; set; }
    
    public TranscriptionJobStatus Status { get; set; }
}

public class CreateSpeechMaticsJobResponseDto
{
    public string Result { get; set; }
}