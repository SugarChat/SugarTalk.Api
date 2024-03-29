using System;
using SugarTalk.Messages.Enums.Meeting.Speak;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Messages.Dto.Meetings.Speak;

public class MeetingSpeakDetailDto
{
    public int Id { get; set; }
    
    public string MeetingNumber { get; set; }
    
    public Guid MeetingRecordId { get; set; }
    
    public string TrackId { get; set; }
    
    public int UserId { get; set; }
    
    public string Username { get; set; }
    
    public long SpeakStartTime { get; set; }
    
    public long? SpeakEndTime { get; set; }
    
    public SpeakStatus SpeakStatus { get; set; }
    
    public string OriginalContent { get; set; }
    
    public string SmartContent { get; set; }
    
    public string OriginalTranslationContent { get; set; }

    public string SmartTranslationContent { get; set; }

    public MeetingSpeakTranslatingStatus TranslationStatus { get; set; }

    public FileTranscriptionStatus FileTranscriptionStatus { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
}