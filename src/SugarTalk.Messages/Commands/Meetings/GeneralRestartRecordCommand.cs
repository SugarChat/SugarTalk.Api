using System;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Commands.Meetings;

public class GeneralRestartRecordCommand : ICommand
{
    public Guid MeetingId { get; set; }

    public Guid MeetingRecordId { get; set; }

    public string Url { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
    
    public int ReTryLimit { get; set; } = 5;
}