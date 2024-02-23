using System.Collections.Generic;

namespace SugarTalk.Messages.Enums.OpenAi;

public class AudioTranscriptionResponseDto
{
    public string Text { get; set; }

    public string Task { get; set; }

    public string Language { get; set; }

    public float Duration { get; set; }

    public List<AudioTranscriptionSegmentDto> Segments { get; set; }
}

public class AudioTranscriptionSegmentDto
{
    public int Id { get; set; }

    public int Seek { get; set; }

    public float Start { get; set; }

    public float End { get; set; }

    public string Text { get; set; }

    public List<int> Tokens { get; set; }

    public float Temperature { get; set; }

    public float Avglogprob { get; set; }

    public float CompressionRatio { get; set; }
        
    public float Nospeechprob { get; set; }

    public bool Transient { get; set; }
}