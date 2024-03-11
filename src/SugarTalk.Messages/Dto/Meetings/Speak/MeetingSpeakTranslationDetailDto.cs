using System;
using SugarTalk.Messages.Dto.Translation;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Messages.Dto.Meetings.Speak;

public class MeetingSpeakTranslationDetailDto
{
    public int Id { get; set; }
    
    public Guid MeetingRecordId { get; set; }

    public int MeetingSpeakDetailId { get; set; }

    public MeetingBackLoadingStatus Status { get; set; }

    public TranslationLanguage Language{ get; set; }

    public string OriginalTranslationContent { get; set; }

    public string SmartTranslationContent { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
}