using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class UpdateMeetingRecordUrlCommand : ICommand
{
    public Guid Id { get; set; }

    public string Url { get; set; }
}

public class UpdateMeetingRecordUrlResponse : SugarTalkResponse
{
}