using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Enums;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class ScheduleMeetingCommand : ICommand
{
    public Guid Id { get; set; }

    public MeetingType MeetingType { get; set; }
}

public class ScheduleMeetingResponse : SugarTalkResponse<MeetingDto>
{
}
