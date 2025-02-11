using System;
using Mediator.Net.Contracts;
using System.Collections.Generic;

namespace SugarTalk.Messages.Commands.SpeechMatics;

public class TranscriptionCallBackCommand : ICommand
{
    public string Format { get; set; }
    
    public SpeechMaticsJobInfoDto Job { get; set; }
    
    public SpeechMaticsMetadataDto Metadata { get; set; }
    
    public List<SpeechMaticsResultDto> Results { get; set; }
}

public class SpeechMaticsJobInfoDto
{
    public DateTime CreatedAt { get; set; }
    
    public string DataName { get; set; }
    
    public int Duration { get; set; }
    
    public string Id { get; set; }
}

public class SpeechMaticsMetadataDto
{
    public DateTime CreatedAt { get; set; }
    
    public string Type { get; set; }
}

public class SpeechMaticsResultDto
{
    public string Channel { get; set; }
    
    public double StartTime { get; set; }
    
    public double EndTime { get; set; }
    
    public string Type { get; set; } 
    
    public List<SpeechMaticsAlternativeDto> Alternatives { get; set; }
}

public class SpeechMaticsAlternativeDto
{
    public string Speaker { get; set; }
    
    public string Content { get; set; }
}