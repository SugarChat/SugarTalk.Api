using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings.Summary;

namespace SugarTalk.Messages.Events.Meeting.Summary;

public class MeetingRecordSummarizedEvent : IEvent
{
    public MeetingSummaryDto Summary { get; set; }
}