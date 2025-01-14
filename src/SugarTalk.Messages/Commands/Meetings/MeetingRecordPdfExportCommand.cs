using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Dto.Translation;

namespace SugarTalk.Messages.Commands.Meetings;

public class MeetingRecordPdfExportCommand : ICommand
{
    public Guid MeetingRecordId { get; set; }
    
    public PdfExportType PdfExportType { get; set; }
    
    public TranslationLanguage TargetLanguage { get; set; }
    
    public string PdfContent { get; set; }
}

public class MeetingRecordPdfExportResponse : SugarTalkResponse<MeetingRecordPdfExportDto>
{
}

public class MeetingRecordPdfExportDto
{
    public string Url { get; set; }
}