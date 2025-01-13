using System.Runtime.CompilerServices;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class MeetingSummaryPDFExportCommand : ICommand
{
    public string PdfContent { get; set; }
}

public class MeetingSummaryPDFExportResponse : SugarTalkResponse<MeetingSummaryPDFExportDto>
{
}

public class MeetingSummaryPDFExportDto
{
    public string Url { get; set; }
}