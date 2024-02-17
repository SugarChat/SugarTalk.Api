using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings.Summary;

namespace SugarTalk.Messages.Commands.Meetings.Summary;

public class ProcessSummaryMeetingCommand : ICommand
{
    public MeetingSummaryBaseInfoDto SummaryInfo { get; set; }
}