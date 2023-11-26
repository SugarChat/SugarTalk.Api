using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Messages.Commands.Meetings;

public class UpdateMeetingCommand : AddOrUpdateMeetingDto, ICommand
{
    public Guid Id { get; set; }
}

public class UpdateMeetingResponse : SugarTalkResponse
{
}