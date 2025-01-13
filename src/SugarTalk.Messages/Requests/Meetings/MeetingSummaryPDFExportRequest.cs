using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Dto.Translation;

namespace SugarTalk.Messages.Requests.Meetings;

public class MeetingSummaryPDFExportRequest : IRequest
{
    public int SummaryId { get; set; }
    
    public TranslationLanguage TargetLanguage { get; set; }
    
    public string PdfContent { get; set; }
}

public class MeetingSummaryPDFExportResponse : SugarTalkResponse<MeetingSummaryPDFExportDto>
{
}

public class MeetingSummaryPDFExportDto
{
    public string Url { get; set; }
}