using System.Runtime.CompilerServices;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class MeetingSummaryPDFExportCommand : ICommand
{
    public string FileName { get; set; }
}

public class MeetingSummaryPDFExportResponse : SugarTalkResponse<MeetingSummaryPDFExportDto>
{
}

public class MeetingSummaryPDFExportDto
{
    public byte[] FileContent { get; set; }
    
    public string FileName { get; set; }
    
    public string ContentType { get; set; }
}