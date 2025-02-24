using System;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Commands.Meetings;

public class CreateSpeechMaticsJobCommand : ICommand
{
    public Guid RecordId { get; set; }

    public string MeetingNumber { get; set; }
}