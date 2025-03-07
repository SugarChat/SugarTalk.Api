using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Translation;

namespace SugarTalk.Messages.Commands.Meetings.Summary;

public class ProcessSummaryMeetingCommand : ICommand
{
    public int MeetingSummaryId { get; set; }
    
    public TranslationLanguage Language { get; set; }

    public Guid? MeetingId { get; set; }

    public bool? IsMetis { get; set; }
}